using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class RaceConditionDemo
    {
        public static async Task RunAsync(ProductRepository productRepo, CancellationToken ct = default)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("    RACE CONDITION DEMO (TASK 8.3)      ");
            Console.WriteLine("========================================");

            int keyboardProductId = 4;
            await ResetStockAsync(productRepo, keyboardProductId, 1, ct);
            Console.WriteLine($"[Setup] Keyboard (Product #{keyboardProductId}) stock set to 1.");

            Console.WriteLine("\n--- Running Unsafe Concurrent Reservation (#10 & #11) ---");

            var task10 = productRepo.ReserveStockUnsafeAsync(keyboardProductId, 1, ct);
            var task11 = productRepo.ReserveStockUnsafeAsync(keyboardProductId, 1, ct);

            await Task.WhenAll(task10, task11);

            Console.WriteLine($"Order #10 Result (Unsafe): {task10.Result}");
            Console.WriteLine($"Order #11 Result (Unsafe): {task11.Result}");

            var finalProductUnsafe = await productRepo.GetByIdAsync(keyboardProductId, ct);
            Console.WriteLine($"[Result] Stock left in file after Unsafe Race: {finalProductUnsafe?.StockQuantity}");

            Console.WriteLine("\n--- Running SAFE Concurrent Reservation after Lock Fix ---");

            await ResetStockAsync(productRepo, keyboardProductId, 1, ct);

            var safeTask10 = productRepo.ReserveStockAsync(keyboardProductId, 1, ct);
            var safeTask11 = productRepo.ReserveStockAsync(keyboardProductId, 1, ct);

            await Task.WhenAll(safeTask10, safeTask11);

            Console.WriteLine($"Order #10 Result (Safe): {safeTask10.Result}");
            Console.WriteLine($"Order #11 Result (Safe): {safeTask11.Result}");

            var finalProductSafe = await productRepo.GetByIdAsync(keyboardProductId, ct);
            Console.WriteLine($"[Result] Stock left in file after Safe Lock: {finalProductSafe?.StockQuantity}");

            Console.WriteLine("========================================");
        }

        private static async Task ResetStockAsync(ProductRepository repo, int productId, int stock, CancellationToken ct)
        {
            var fileStore = new JsonFileStore();
            var products = await fileStore.ReadAsync<Product>("products.json", ct);
            var p = products.FirstOrDefault(x => x.Id == productId);
            if (p != null)
            {
                p.StockQuantity = stock;
                await fileStore.WriteAsync("products.json", products, ct);
            }
        }
    }
}