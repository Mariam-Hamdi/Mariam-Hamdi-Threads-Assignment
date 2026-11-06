using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public static class Phase1Runner
    {
        public static async Task RunAsync(
            SyncOrderValidator syncValidator,
            OrderValidator asyncValidator,
            OrderRepository orderRepo,
            CancellationToken ct = default)
        {
            Console.WriteLine("=== Starting Phase 1 Benchmark & Validation ===");

            var fileStore = new JsonFileStore();

            var initialOrders = fileStore.ReadSync<Order>(DataFiles.IncomingOrders);
            Console.WriteLine($"[Phase1Runner] Loaded {initialOrders.Count} orders from {DataFiles.IncomingOrders}");

            void ResetOrdersFile()
            {
                fileStore.WriteSync(DataFiles.Orders, new List<Order>());
            }

            // ==========================================
            // --- 2. SYNCHRONOUS RUN ---
            // ==========================================
            ResetOrdersFile();

            var stopwatch = Stopwatch.StartNew();

            foreach (var order in initialOrders)
            {
                var orderToValidate = JsonSerializerCopy(order);
                syncValidator.ValidateOrder(orderToValidate);
            }

            stopwatch.Stop();
            double syncTimeSeconds = stopwatch.Elapsed.TotalSeconds;

            // ==========================================
            // --- 3. ASYNCHRONOUS RUN ---
            // ==========================================
            ResetOrdersFile(); 

            stopwatch.Restart();

            foreach (var order in initialOrders)
            {
                var orderToValidate = JsonSerializerCopy(order);
                await asyncValidator.ValidateOrderAsync(orderToValidate, ct);
            }

            stopwatch.Stop();
            double asyncTimeSeconds = stopwatch.Elapsed.TotalSeconds;

            var validatedOrders = await orderRepo.GetByStatusAsync(OrderStatus.Validated, ct);
            var rejectedOrders = await orderRepo.GetByStatusAsync(OrderStatus.Rejected, ct);
            var allSavedOrders = await orderRepo.GetAllAsync(ct);

            Console.WriteLine("\n===== Validation Summary =====");
            Console.WriteLine($"Total Orders: {initialOrders.Count}");
            Console.WriteLine($"Validated:    {validatedOrders.Count}");
            Console.WriteLine($"Rejected:     {rejectedOrders.Count}");

            foreach (var rejected in rejectedOrders)
            {
                Console.WriteLine($"- Order #{rejected.Id}: {rejected.FailureReason}");
            }

            Console.WriteLine($"Sync Time:  {syncTimeSeconds:F1} seconds");
            Console.WriteLine($"Async Time: {asyncTimeSeconds:F1} seconds");
            Console.WriteLine($"orders.json now contains {allSavedOrders.Count} records.");
            Console.WriteLine("==============================");
        }

        private static Order JsonSerializerCopy(Order source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<Order>(json)!;
        }
    }
}