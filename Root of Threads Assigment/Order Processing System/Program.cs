using System;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Services;

namespace Order_Processing_System
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var fileStore = new JsonFileStore();

            var seeder = new DataSeeder(fileStore);
            await seeder.ResetAsync(default);

            var customerRepo = new CustomerRepository(fileStore);
            var productRepo = new ProductRepository(fileStore);
            var couponRepo = new CouponRepository(fileStore);
            var orderRepo = new OrderRepository(fileStore);

            var syncCustomerRepo = new SyncCustomerRepository(fileStore);
            var syncProductRepo = new SyncProductRepository(fileStore);
            var syncCouponRepo = new SyncCouponRepository(fileStore);
            var reportGenerator = new ReportGenerator(orderRepo, productRepo);

            var syncValidator = new SyncOrderValidator(syncCustomerRepo, syncProductRepo, syncCouponRepo, orderRepo);
            var asyncValidator = new OrderValidator(customerRepo, productRepo, couponRepo, orderRepo);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("========================================");
            Console.WriteLine("    ORDER PROCESSING SYSTEM - FULL RUN  ");
            Console.WriteLine("========================================");

            var cts = new CancellationTokenSource();

            try
            {
                await Phase2Runner.RunAsync(productRepo, customerRepo, couponRepo, cts.Token);

                await RaceConditionDemo.RunAsync(productRepo, cts.Token);

                await seeder.ResetAsync(cts.Token);

                await Phase3Runner.RunAsync(productRepo, customerRepo, couponRepo, orderRepo, cts.Token);

                Console.WriteLine("\n[Report] Triggering report generation...");
                Task reportTask = reportGenerator.GenerateAsync(cts.Token);

                Console.WriteLine("Generating report...");

                await reportTask;
                Console.WriteLine("[Report] Report generation completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Main Error] An unexpected error occurred: {ex.Message}");
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("       ALL EXECUTIONS COMPLETED         ");
            Console.WriteLine("========================================");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}