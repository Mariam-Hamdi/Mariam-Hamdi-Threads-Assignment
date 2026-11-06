using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Services
{
    public class SimulationHelper
    {
        public static async Task SimulateDbQueryAsync(string callername, CancellationToken ct)
        {
            AppLog.Info($"{callername} -> Opening connection...");
            await Task.Delay(200, ct);

            int queryDelay = Random.Shared.Next(300, 601);

            AppLog.Info($"{callername}  -> Executing query for Customer #3...");
            await Task.Delay(queryDelay, ct);
        }

        public static async Task SimulateDbWriteAsync(string callername, Func<Task> writeAction, CancellationToken ct)
        {
            AppLog.Info($"{callername} -> Opening connection...");
            await Task.Delay(200, ct);

            AppLog.Info($"{callername} -> Beginning transaction...");
            await Task.Delay(100, ct);

            AppLog.Info($"{callername} -> Performing Insert/Update...");
            await Task.Delay(400, ct);

            await writeAction();
            AppLog.Info($"{callername} -> Committing transaction...");
            await Task.Delay(200, ct);
        }

        public static async Task SimulateExternalApiAsync(string callername, CancellationToken ct)
        {
            AppLog.Info($"{callername} -> DNS / Handshake...");
            await Task.Delay(140, ct);

            AppLog.Info($"{callername} -> Sending request...");
            await Task.Delay(300, ct);

            int queryDelay = Random.Shared.Next(500, 1501);
            AppLog.Info($"{callername} -> Waiting for response");
            await Task.Delay(queryDelay, ct);
        }

        public static async Task SimulatePaymentGatewayAsync(string callername, CancellationToken ct)
        {
            AppLog.Info($"{callername} -> Authorizing ");
            await Task.Delay(800, ct);

            AppLog.Info($"{callername} -> Running  Fraud check ");
            await Task.Delay(500, ct);

            AppLog.Info($"{callername} -> Running Capturing  ");
            await Task.Delay(700, ct);
        }

        public static async Task SimulateEmailAsync(string callerName, CancellationToken ct)
        {
            AppLog.Info($"{callerName} -> Rendering email template...");
            await Task.Delay(200, ct);

            // Connect to SMTP (300 ms)
            AppLog.Info($"{callerName} -> Connecting to SMTP server...");
            await Task.Delay(300, ct);

            // Send (500 ms)
            AppLog.Info($"{callerName} -> Sending email...");
            await Task.Delay(500, ct);
        }

        // =================================
        // sync method 
        public static void SimulateDbQuerySync(string callername)
        {
            AppLog.Info($"{callername} -> Opening connection...");
            Thread.Sleep(200);

            int queryDelay = Random.Shared.Next(300, 601);

            AppLog.Info($"{callername}  -> Executing query for Customer #3...");
            Thread.Sleep(queryDelay);
        }

        public static void SimulateDbWriteSync(string callername, Action writeAction)
        {
            AppLog.Info($"{callername} -> Opening connection...");
            Thread.Sleep(200);

            AppLog.Info($"{callername} -> Beginning transaction...");
            Thread.Sleep(100);

            AppLog.Info($"{callername} -> Performing Insert/Update...");
            Thread.Sleep(400);

            writeAction();

            AppLog.Info($"{callername} -> Committing transaction...");
            Thread.Sleep(200);
        }

        public static void SimulateExternalApiSync(string callername)
        {
            AppLog.Info($"{callername} -> DNS / Handshake...");
            Thread.Sleep(140);

            AppLog.Info($"{callername} -> Sending request...");
            Thread.Sleep(300);

            int queryDelay = Random.Shared.Next(500, 1501);
            AppLog.Info($"{callername} -> Waiting for response");
            Thread.Sleep(queryDelay);
        }

        public static void SimulatePaymentGatewaySync(string callername)
        {
            AppLog.Info($"{callername} -> Authorizing ");
            Thread.Sleep(800);

            AppLog.Info($"{callername} -> Running  Fraud check ");
            Thread.Sleep(500);

            AppLog.Info($"{callername} -> Running Capturing  ");
            Thread.Sleep(700);
        }

        public static void SimulateEmailSync(string callerName)
        {
            AppLog.Info($"{callerName} -> Rendering email template...");
            Thread.Sleep(200);

            // Connect to SMTP (300 ms)
            AppLog.Info($"{callerName} -> Connecting to SMTP server...");
            Thread.Sleep(300);

            // Send (500 ms)
            AppLog.Info($"{callerName} -> Sending email...");
            Thread.Sleep(500);
        }
    }
}