using System.Collections.Concurrent;
namespace ViewsAndEmails;

public sealed class AnalyticsAndEmailJobs
{
    public readonly ConcurrentDictionary<string, int> Views = new();
    public readonly ConcurrentQueue<string> EmailJobs = new();
    public readonly List<string> SentEmails = new();

    readonly object _sentSync = new();
    public volatile bool ProducersDone;
   
    //########## this solution of first problem 
    public void RecordView(string product)
    {
        Views.AddOrUpdate(product,addValue:1, // if key missing
            updateValueFactory: (key, oldValue) => oldValue + 1); // if exist add 1 on old value
    }

    public void PlaceOrder(int orderId)
    {
        EmailJobs.Enqueue($"Email: order {orderId}");
    }

    public void EmailWorker()
    {
        while (true)
        {
            try
            {
                // ########## this solution of second problem 
                if (EmailJobs.TryDequeue(out string? job))// to collection safe 
                { 
                    lock (_sentSync) // to businees logic safe 
                        SentEmails.Add(job);
                    continue;
                }

                if (ProducersDone)
                    return;

                Thread.Sleep(1);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("InvalidOperationException");
                if (ProducersDone)
                    return;
            }
        }
    }
}
