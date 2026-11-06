using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class Phase4Runner
    {
        public static async Task RunAllAsync(CancellationToken ct = default)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("    STARTING PHASE 4 — SCALE-UP");
            Console.WriteLine("========================================");

            var fileStore = new JsonFileStore();
            var customerRepo = new CustomerRepository(fileStore);
            var orderRepo = new OrderRepository(fileStore);

         
            Console.WriteLine("\n[Running Task 10.1: Live Checkout Progress simulated]");
           
            Console.WriteLine("\n[Running Task 10.2: Customer Dashboard]");
            var dashboardService = new CustomerDashboardService(customerRepo, orderRepo, fileStore, failPaymentsForCustomerIds: new[] { 99 });

            var (res3, time3, ex3) = await dashboardService.LoadAsync(3, ct);
            Console.WriteLine($"Customer #3 loaded in {time3.TotalMilliseconds} ms. Orders: {res3.Orders.Count}");

            var (res99, time99, ex99) = await dashboardService.LoadAsync(99, ct);
            Console.WriteLine($"Customer #99 loaded in {time99.TotalMilliseconds} ms with {ex99.Count} errors:");
            foreach (var err in ex99) Console.WriteLine($" - {err.Message}");

            Console.WriteLine("\n[Running Task 10.3: Fastest Shipping Quotes]");
            var carriers = new List<IShippingCarrier> { new FastShip(), new CairoExpress(), new NileCourier() };
            var sampleOrder = new Order { Id = 1, SubTotal = 500m, TotalQuantity = 2, TotalWeightKg = 1.5m };
            var (carrier, price, ms, cancelled) = await ShippingQuoteService.GetFastestQuoteAsync(sampleOrder, carriers, ct);
            Console.WriteLine($"Winning Carrier: {carrier}, Price: ${price}, Time: {ms} ms. Cancelled: {string.Join(", ", cancelled)}");

            ScaleUpTasks.RunFraudScoring(ct);

            IEmailService emailService = new EmailService(new Dictionary<int, int>(), fileStore);
            await ScaleUpTasks.RunEmailResendAsync(emailService, ct);

            Console.WriteLine("\n========================================");
            Console.WriteLine("    PHASE 4 COMPLETED SUCCESSFULLY");
            Console.WriteLine("========================================");
        }
    }
}