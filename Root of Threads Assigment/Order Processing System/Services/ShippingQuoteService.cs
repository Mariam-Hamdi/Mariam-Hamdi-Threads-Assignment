using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public interface IShippingCarrier
    {
        string Name { get; }
        Task<(string Carrier, decimal Price, long ElapsedMs)> GetQuoteAsync(Order order, CancellationToken ct);
    }

    public class FastShip : IShippingCarrier
    {
        public string Name => "FastShip";
        private readonly HashSet<int> _failOrders;
        public FastShip(IEnumerable<int>? failForOrderIds = null) => _failOrders = new(failForOrderIds ?? Array.Empty<int>());

        public async Task<(string Carrier, decimal Price, long ElapsedMs)> GetQuoteAsync(Order order, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await Task.Delay(150, ct); 
            await Task.Delay(Random.Shared.Next(500, 1500), ct); 

            if (_failOrders.Contains(order.Id)) throw new ShippingProviderUnavailableException("FastShip failed");

            decimal price = order.SubTotal < 1000 ? (5.0m + 1.5m * order.TotalQuantity + 2.0m * (decimal)order.TotalWeightKg) * 1.00m : 0m;
            sw.Stop();
            return (Name, Math.Round(price, 2), sw.ElapsedMilliseconds);
        }
    }

    public class CairoExpress : IShippingCarrier
    {
        public string Name => "CairoExpress";
        private readonly HashSet<int> _failOrders;
        public CairoExpress(IEnumerable<int>? failForOrderIds = null) => _failOrders = new(failForOrderIds ?? Array.Empty<int>());

        public async Task<(string Carrier, decimal Price, long ElapsedMs)> GetQuoteAsync(Order order, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await Task.Delay(150, ct);
            await Task.Delay(Random.Shared.Next(300, 2000), ct);

            if (_failOrders.Contains(order.Id) || order.Id % 2 == 0) throw new ShippingProviderUnavailableException("CairoExpress unavailable");

            decimal price = order.SubTotal < 1000 ? (5.0m + 1.5m * order.TotalQuantity + 2.0m * (decimal)order.TotalWeightKg) * 0.95m : 0m;
            sw.Stop();
            return (Name, Math.Round(price, 2), sw.ElapsedMilliseconds);
        }
    }

    public class NileCourier : IShippingCarrier
    {
        public string Name => "NileCourier";
        private readonly HashSet<int> _failOrders;
        public NileCourier(IEnumerable<int>? failForOrderIds = null) => _failOrders = new(failForOrderIds ?? Array.Empty<int>());

        public async Task<(string Carrier, decimal Price, long ElapsedMs)> GetQuoteAsync(Order order, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await Task.Delay(150, ct);
            await Task.Delay(Random.Shared.Next(800, 1200), ct);

            if (_failOrders.Contains(order.Id)) throw new ShippingProviderUnavailableException("NileCourier failed");

            decimal price = order.SubTotal < 1000 ? (5.0m + 1.5m * order.TotalQuantity + 2.0m * (decimal)order.TotalWeightKg) * 1.10m : 0m;
            sw.Stop();
            return (Name, Math.Round(price, 2), sw.ElapsedMilliseconds);
        }
    }

    public class ShippingQuoteService
    {
        public static async Task<(string Carrier, decimal Price, long ElapsedMs, List<string> CancelledCarriers)> GetFastestQuoteAsync(
            Order order, List<IShippingCarrier> carriers, CancellationToken ct)
        {
            using var budgetCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1200));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, budgetCts.Token);

            var cancelledCarriers = new List<string>();
            var activeTasks = new List<Task<(string Carrier, decimal Price, long ElapsedMs)>>();
            var carrierNamesMap = new Dictionary<Task<(string Carrier, decimal Price, long ElapsedMs)>, string>();

            foreach (var carrier in carriers)
            {
                var task = carrier.GetQuoteAsync(order, combinedCts.Token);
                activeTasks.Add(task);
                carrierNamesMap[task] = carrier.Name;
            }

            while (activeTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(activeTasks);
                activeTasks.Remove(finishedTask);

                try
                {
                    var result = await finishedTask;
                    combinedCts.Cancel();
                    foreach (var remaining in activeTasks)
                    {
                        if (carrierNamesMap.TryGetValue(remaining, out var name))
                            cancelledCarriers.Add(name);
                    }
                    return (result.Carrier, result.Price, result.ElapsedMs, cancelledCarriers);
                }
                catch
                {
                    string failedName = carrierNamesMap.TryGetValue(finishedTask, out var name) ? name : "Unknown";
                    cancelledCarriers.Add($"{failedName} (Failed)");
                }
            }

            if (budgetCts.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                return ("Flat rate", 25.00m, 1200, cancelledCarriers);
            }

            throw new ShippingProviderUnavailableException("All carriers failed");
        }
    }
}