using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Services
    {
        public interface IPaymentGateway
        {
            Task<string> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct);
            Task RefundAsync(string transactionId, CancellationToken ct);
        }

        public class PaymentGateway : IPaymentGateway
        {
            private readonly HashSet<int> _timeoutOrderIds;
            private readonly HashSet<int> _declineOrderIds;
            private readonly JsonFileStore _fileStore;

            public PaymentGateway(IEnumerable<int> timeoutOrderIds, IEnumerable<int> declineOrderIds, JsonFileStore fileStore)
            {
                _timeoutOrderIds = new HashSet<int>(timeoutOrderIds);
                _declineOrderIds = new HashSet<int>(declineOrderIds);
                _fileStore = fileStore;
            }

            public async Task<string> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct)
            {
                await SimulationHelper.SimulateExternalApiAsync($"PaymentGateway-Order#{orderId}", ct);

                if (_timeoutOrderIds.Contains(orderId))
                {
                    await Task.Delay(5000, ct); 
                    throw new TimeoutException("Payment gateway timeout (3s).");
                }

                if (_declineOrderIds.Contains(orderId))
                {
                    await RecordPaymentAsync(orderId, amount, "Declined", "Card declined by issuer", ct);
                    throw new InvalidOperationException("Card declined by issuer");
                }

                string transactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

                await RecordPaymentAsync(orderId, amount, "Captured", transactionId, ct);

                return transactionId;
            }

            public async Task RefundAsync(string transactionId, CancellationToken ct)
            {
                await SimulationHelper.SimulateExternalApiAsync($"PaymentRefund-{transactionId}", ct);

                var payments = await _fileStore.ReadAsync<PaymentRecord>("payments.json", ct);
                var record = payments.Find(p => p.TransactionId == transactionId);

                if (record != null)
                {
                    record.Status = "Refunded";
                    await _fileStore.WriteAsync("payments.json", payments, ct);
                }
            }

            private async Task RecordPaymentAsync(int orderId, decimal amount, string status, string transactionIdOrReason, CancellationToken ct)
            {
                var payments = await _fileStore.ReadAsync<PaymentRecord>("payments.json", ct) ?? new List<PaymentRecord>();

                var paymentRecord = new PaymentRecord
                {
                    OrderId = orderId,
                    Amount = amount,
                    Status = status, 
                    TransactionId = status == "Captured" ? transactionIdOrReason : null,
                    FailureReason = status == "Declined" ? transactionIdOrReason : null,
                    CreatedAt = DateTime.UtcNow
                };

                payments.Add(paymentRecord);
                await _fileStore.WriteAsync("payments.json", payments, ct);
            }
        }

        public class PaymentRecord
        {
            public int OrderId { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; } 
            public string? TransactionId { get; set; }
            public string? FailureReason { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }

