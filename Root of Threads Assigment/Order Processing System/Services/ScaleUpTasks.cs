using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class ScaleUpTasks
    {
        public static int Score(int orderId)
        {
            long h = orderId;
            for (int i = 0; i < 100_000; i++)
                h = (h * 31 + i) % 1_000_000_007;
            return (int)(h % 100);
        }

        public static void RunFraudScoring(CancellationToken ct)
        {
            Console.WriteLine("\n--- Task 10.4 A: Fraud Scoring (10,000 Orders) ---");

            var orders = new List<int>(10_000);
            for (int i = 1; i <= 10_000; i++) orders.Add(i);

            var swSeq = Stopwatch.StartNew();
            var seqFlagged = new List<int>();
            foreach (var id in orders)
            {
                if (Score(id) >= 90) seqFlagged.Add(id);
            }
            swSeq.Stop();

            var swPar = Stopwatch.StartNew();
            var parFlagged = new ConcurrentBag<int>();
            var distinctThreads = new ConcurrentDictionary<int, bool>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = ct
            };

            Parallel.ForEach(orders, parallelOptions, id =>
            {
                distinctThreads[Environment.CurrentManagedThreadId] = true;
                if (Score(id) >= 90)
                {
                    parFlagged.Add(id);
                }
            });
            swPar.Stop();

            Console.WriteLine($"Flagged Count: {seqFlagged.Count}");
            Console.WriteLine($"Sequential Time: {swSeq.ElapsedMilliseconds} ms");
            Console.WriteLine($"Parallel Time: {swPar.ElapsedMilliseconds} ms");
            Console.WriteLine($"Speed-up: {(double)swSeq.ElapsedMilliseconds / Math.Max(1, swPar.ElapsedMilliseconds):F2}x");
            Console.WriteLine($"Distinct Threads Used: {distinctThreads.Count}");
            Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        }

        public static async Task RunEmailResendAsync(IEmailService emailService, CancellationToken ct)
        {
            Console.WriteLine("\n--- Task 10.4 B: Email Re-send (30 Emails, Max 4 Concurrent) ---");

            var emailsToProcess = new List<int>(30);
            for (int i = 1001; i <= 1030; i++) emailsToProcess.Add(i);

            int currentConcurrent = 0;
            int maxConcurrentObserved = 0;
            var sw = Stopwatch.StartNew();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(emailsToProcess, parallelOptions, async (orderId, token) =>
            {
                int active = Interlocked.Increment(ref currentConcurrent);
                lock (typeof(ScaleUpTasks))
                {
                    if (active > maxConcurrentObserved)
                        maxConcurrentObserved = active;
                }

                try
                {
                    var dummyOrder = new Order { Id = orderId, TotalAmount = 100m };
                    var dummyCustomer = new Customer { Id = orderId, Email = $"user{orderId}@test.com", Name = $"User {orderId}", IsActive = true };

                    await Task.Delay(1000, token);
                    await emailService.SendConfirmationAsync(dummyOrder, dummyCustomer, token);
                }
                finally
                {
                    Interlocked.Decrement(ref currentConcurrent);
                }
            });

            sw.Stop();
            Console.WriteLine($"Total Time: {sw.ElapsedMilliseconds} ms (Expected ~8000 ms)");
            Console.WriteLine($"Highest Concurrent Sends Observed: {maxConcurrentObserved} (Expected: exactly 4)");
        }
    }
}