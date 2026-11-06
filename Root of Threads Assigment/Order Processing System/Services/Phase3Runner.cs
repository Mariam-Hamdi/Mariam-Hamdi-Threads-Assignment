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
    public class Phase3Runner
    {
        public static async Task RunAsync(
            ProductRepository productRepo,
            CustomerRepository customerRepo,
            CouponRepository couponRepo,
            OrderRepository orderRepo,
            CancellationToken ct = default)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("    STARTING PHASE 3 — FULL WORKFLOW PIPELINE");
            Console.WriteLine("========================================");

            var stopwatch = Stopwatch.StartNew();

            var fileStore = new JsonFileStore();
            var dataSeeder = new DataSeeder(fileStore);

            await dataSeeder.ResetAsync(ct);

            var incomingOrders = await fileStore.ReadAsync<Order>("incoming_orders.json", ct) ?? new List<Order>();

            if (incomingOrders.Count == 0)
            {
                Console.WriteLine("[Phase3Runner] No incoming orders found.");
                return;
            }

            IPaymentGateway paymentGateway = new PaymentGateway(
                timeoutOrderIds: new[] { 5 },
                declineOrderIds: new[] { 8 },
                fileStore: fileStore
            );

            IEmailService emailService = new EmailService(
                failAttempts: new Dictionary<int, int> { { 3, 2 }, { 17, int.MaxValue } },
                fileStore: fileStore
            );

            var orderPricer = new OrderPricer(
                failShippingOrderIds: Array.Empty<int>(),
                productRepository: productRepo,
                customerRepository: customerRepo,
                couponRepository: couponRepo
            );

            var processor = new OrderProcessor(
                customerRepository: customerRepo,
                productRepository: productRepo,
                orderPricer: orderPricer,
                paymentGateway: paymentGateway,
                orderRepository: orderRepo,
                emailService: emailService,
                fileStore: fileStore
            );

            Console.WriteLine($"[Phase3Runner] Processing {incomingOrders.Count} orders concurrently...");

            var processingTasks = new List<Task<bool>>();

            foreach (var order in incomingOrders)
            {
                processingTasks.Add(processor.ProcessOrderAsync(order, ct));
            }

            // انتظار إتمام جميع المهام
            await Task.WhenAll(processingTasks);
            stopwatch.Stop();

            Console.WriteLine($"\n=== All Orders Processed in {stopwatch.ElapsedMilliseconds} ms ===");

            var finalOrders = await orderRepo.GetAllAsync(ct);

            Console.WriteLine("\n{0,-8} | {1,-15} | {2,-10} | {3}", "Order #", "Status", "Total ($)", "Reason / Transaction");
            Console.WriteLine(new string('-', 75));

            foreach (var ord in finalOrders.OrderBy(o => o.Id))
            {
                string info = ord.Status == OrderStatus.Completed ? ord.TransactionId ?? "" : ord.FailureReason ?? "";
                Console.WriteLine($"#{ord.Id,-7} | {ord.Status,-15} | {ord.TotalAmount,-10:F2} | {info}");
            }

            Console.WriteLine("\n===== Summary by Status =====");
            var statusCounts = finalOrders.GroupBy(o => o.Status).Select(g => new { Status = g.Key, Count = g.Count() });
            foreach (var sc in statusCounts)
            {
                Console.WriteLine($"{sc.Status}: {sc.Count}");
            }
            Console.WriteLine("==========================================");
        }
    }
}