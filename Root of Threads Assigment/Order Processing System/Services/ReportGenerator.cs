using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class ReportGenerator
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;

        public ReportGenerator(OrderRepository orderRepository, ProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task GenerateAsync(CancellationToken ct = default)
        {
            var orders = await _orderRepository.GetAllAsync(ct);

            if (orders == null || orders.Count == 0)
            {
                Console.WriteLine("[ReportGenerator] No orders found to generate report.");
                return;
            }

            int totalOrders = orders.Count;
            int completedCount = orders.Count(o => o.Status == OrderStatus.Completed);
            int rejectedCount = orders.Count(o => o.Status == OrderStatus.Rejected);
            int failedCount = orders.Count(o => o.Status == OrderStatus.Failed);
            int paymentFailedCount = orders.Count(o => o.Status.ToString() == "PaymentFailed");

            decimal totalRevenue = orders
                .Where(o => o.Status == OrderStatus.Completed)
                .Sum(o => o.TotalAmount);

            decimal averageOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0;

                var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed);

            var topProductQuery = completedOrders
                .SelectMany(o => o.Items)
                .GroupBy(item => item.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ThenBy(x => x.ProductId) 
                .FirstOrDefault();

            string topProductName = "N/A";
            int topProductQty = 0;

            if (topProductQuery != null)
            {
                topProductQty = topProductQuery.TotalQuantity;
                var product = await _productRepository.GetByIdAsync(topProductQuery.ProductId, ct);
                topProductName = product?.Name ?? $"Product #{topProductQuery.ProductId}";
            }

            var reportData = new
            {
                GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TotalOrders = totalOrders,
                CompletedOrders = completedCount,
                RejectedOrders = rejectedCount,
                FailedOrders = failedCount,
                PaymentFailedOrders = paymentFailedCount,
                TotalRevenue = totalRevenue,
                AverageOrderValue = averageOrderValue,
                TopSellingProduct = $"{topProductName}, {topProductQty} units"
            };

            string reportsDirectory = Path.Combine("Data", "reports");
            Directory.CreateDirectory(reportsDirectory); 

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(reportsDirectory, $"report_{timestamp}.json");

            string jsonContent = JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, jsonContent, ct);

            Console.WriteLine("\n========================================");
            Console.WriteLine("          SYSTEM PERFORMANCE REPORT     ");
            Console.WriteLine("========================================");
            Console.WriteLine($"Total Orders Evaluated : {totalOrders}");
            Console.WriteLine($" - Completed           : {completedCount}");
            Console.WriteLine($" - Rejected            : {rejectedCount}");
            Console.WriteLine($" - Failed              : {failedCount}");
            Console.WriteLine($"Total Revenue          : ${totalRevenue:F2}");
            Console.WriteLine($"Average Order Value    : ${averageOrderValue:F2}");
            Console.WriteLine($"Top Selling Product    : {topProductName} ({topProductQty} units)");
            Console.WriteLine($"Report Saved To        : {filePath}");
            Console.WriteLine("========================================");
        }
    }
}