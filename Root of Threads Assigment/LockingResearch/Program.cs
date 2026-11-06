using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static readonly object _lockObj = new();
    private static int _counter = 0;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Testing Locking Mechanisms ===");
        //lock
        var task1 = Task.Run(() => IncrementWithLock());
        var task2 = Task.Run(() => IncrementWithLock());
        //monitor
        var task3 = Task.Run(() => IncrementWithMonitor());
        var task4 = Task.Run(() => IncrementWithMonitor());

        await Task.WhenAll(task1, task2, task3, task4);

        Console.WriteLine($"Final Counter Value (Should be 4): {_counter}");
    }

    public static void IncrementWithLock()
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (_lockObj)
            {
                _counter++;
            }
        }
        Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Finished IncrementWithLock");
    }

    public static void IncrementWithMonitor()
    {
        for (int i = 0; i < 1000; i++)
        {
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _counter++;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_lockObj);
                }
            }
        }
        Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Finished IncrementWithMonitor");
    }
}