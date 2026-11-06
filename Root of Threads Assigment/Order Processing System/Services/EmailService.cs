using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public interface IEmailService
    {
        Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct);
        Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct);
    }

    public class EmailService : IEmailService
    {
        private readonly Dictionary<int, int> _failAttempts;
        private readonly JsonFileStore _fileStore;

        public EmailService(IDictionary<int, int> failAttempts, JsonFileStore fileStore)
        {
            _failAttempts = new Dictionary<int, int>(failAttempts);
            _fileStore = fileStore;
        }

        public async Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct)
        {
            await SimulationHelper.SimulateExternalApiAsync($"Email-Order#{order.Id}", ct);

            if (_failAttempts.TryGetValue(order.Id, out int remainingFails))
            {
                if (remainingFails > 0)
                {
                    _failAttempts[order.Id] = remainingFails - 1;
                    throw new SmtpUnavailableException($"SMTP server unavailable for order #{order.Id}");
                }
            }

            await RecordEmailAsync(order.Id, customer.Email, "Sent", 1, null, ct);
        }

        public async Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct)
        {
            int maxRetries = 3;
            int[] delayMilliseconds = { 500, 1000, 2000 };
            int attempts = 0;
            Exception? lastError = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                attempts++;
                try
                {
                    await SendConfirmationAsync(order, customer, ct);
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"[EmailService] Order #{order.Id} -> Attempt {attempts} failed: {ex.Message}");

                    if (i < maxRetries)
                    {
                        await Task.Delay(delayMilliseconds[i], ct);
                    }
                }
            }

            Console.WriteLine($"[EmailService] Email for Order #{order.Id} queued for retry later.");
            await RecordEmailRetryAsync(order.Id, customer.Email, attempts, lastError?.Message ?? "Unknown error", ct);
        }

        private async Task RecordEmailAsync(int orderId, string email, string status, int attempts, string? lastError, CancellationToken ct)
        {
            var emails = await _fileStore.ReadAsync<EmailRecord>("emails.json", ct) ?? new List<EmailRecord>();

            emails.Add(new EmailRecord
            {
                OrderId = orderId,
                Email = email,
                Status = status,
                Attempts = attempts,
                LastError = lastError,
                CreatedAt = DateTime.UtcNow
            });

            await _fileStore.WriteAsync("emails.json", emails, ct);
        }

        private async Task RecordEmailRetryAsync(int orderId, string email, int attempts, string lastError, CancellationToken ct)
        {
            var retries = await _fileStore.ReadAsync<EmailRetryRecord>("email_retry.json", ct) ?? new List<EmailRetryRecord>();

            retries.Add(new EmailRetryRecord
            {
                OrderId = orderId,
                Email = email,
                Attempts = attempts,
                LastError = lastError,
                CreatedAt = DateTime.UtcNow
            });

            await _fileStore.WriteAsync("email_retry.json", retries, ct);
        }
    }

    public class EmailRecord
    {
        public int OrderId { get; set; }
        public required string Email { get; set; }
        public required string Status { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmailRetryRecord
    {
        public int OrderId { get; set; }
        public required string Email { get; set; }
        public int Attempts { get; set; }
        public required string LastError { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SmtpUnavailableException : Exception
    {
        public SmtpUnavailableException(string message) : base(message) { }
    }
}