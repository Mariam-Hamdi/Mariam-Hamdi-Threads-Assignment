using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public static class Phase2Runner
    {
        public static async Task RunAsync(
            ProductRepository productRepo,
            CustomerRepository customerRepo,
            CouponRepository couponRepo,
            CancellationToken ct = default)
        {
            Console.WriteLine("=== Starting Phase 2 — Concurrent Pricing & Benchmark ===");

            var fileStore = new JsonFileStore();
            var incomingOrders = fileStore.ReadSync<Order>(DataFiles.IncomingOrders);

            if (incomingOrders.Count == 0)
            {
                Console.WriteLine("[Phase2Runner] No orders found to process.");
                return;
            }

            var sampleOrderForBenchmark = incomingOrders.Find(o => o.Id == 1) ?? incomingOrders[0];

            var failShippingIds = new List<int> { 7 };
            var orderPricer = new OrderPricer(failShippingIds, productRepo, customerRepo, couponRepo);

       
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var orderSeq = JsonSerializerCopy(sampleOrderForBenchmark);
                await orderPricer.PriceOrderSequentialAsync(orderSeq, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sequential Error] {ex.Message}");
            }
            stopwatch.Stop();
            double sequentialTime = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch.Restart();
            try
            {
                var orderConcurrent = JsonSerializerCopy(sampleOrderForBenchmark);
                await orderPricer.PriceOrderAsync(orderConcurrent, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Concurrent Error] {ex.Message}");
            }
            stopwatch.Stop();
            double concurrentTime = stopwatch.Elapsed.TotalMilliseconds;

            Console.WriteLine("\n--- Testing Shipping Provider Failure for Order #7 ---");
            var orderSeven = incomingOrders.Find(o => o.Id == 7);
            if (orderSeven != null)
            {
                try
                {
                    var orderToFail = JsonSerializerCopy(orderSeven);
                    await orderPricer.PriceOrderAsync(orderToFail, ct);
                }
                catch (ShippingProviderUnavailableException ex)
                {
                    Console.WriteLine($"[Caught Expected Exception Successfully]: {ex.Message} for Order #7");
                }
            }
            else
            {
                Console.WriteLine("Order #7 not found in incoming orders list.");
            }

          
            Console.WriteLine("\n===== Phase 2 Benchmark Summary (Order #1) =====");
            Console.WriteLine($"Sequential Pricing Time: {sequentialTime:F2} ms");
            Console.WriteLine($"Concurrent Pricing Time: {concurrentTime:F2} ms");
            Console.WriteLine("================================================");
        }

        private static Order JsonSerializerCopy(Order source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<Order>(json)!;
        }
    }
}