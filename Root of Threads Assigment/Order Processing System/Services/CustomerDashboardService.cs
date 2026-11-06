using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class DashboardResult
    {
        public Customer? Customer { get; set; }
        public List<Order> Orders { get; set; } = new();
        public List<PaymentRecord> Payments { get; set; } = new();
        public List<EmailRecord> Emails { get; set; } = new();
    }

    public class CustomerDashboardService
    {
        private readonly CustomerRepository _customerRepo;
        private readonly OrderRepository _orderRepo;
        private readonly JsonFileStore _fileStore;
        private readonly HashSet<int> _failPaymentsCustomerIds;

        public CustomerDashboardService(
            CustomerRepository customerRepo,
            OrderRepository orderRepo,
            JsonFileStore fileStore,
            IEnumerable<int>? failPaymentsForCustomerIds = null)
        {
            _customerRepo = customerRepo;
            _orderRepo = orderRepo;
            _fileStore = fileStore;
            _failPaymentsCustomerIds = new HashSet<int>(failPaymentsForCustomerIds ?? Array.Empty<int>());
        }

        public async Task<(DashboardResult Result, TimeSpan Elapsed, List<Exception> Exceptions)> LoadAsync(int customerId, CancellationToken ct)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var exceptions = new List<Exception>();

            Customer? customer = null;
            List<Order> orders = new();
            List<PaymentRecord> payments = new();
            List<EmailRecord> emails = new();

            var profileTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, ct); 
                    customer = await _customerRepo.GetByIdAsync(customerId, ct);
                    if (customer == null)
                        throw new ArgumentException($"Customer #{customerId} not found");
                }
                catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
            }, ct);

            var ordersTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(400, ct);
                    var allOrders = await _orderRepo.GetAllAsync(ct);
                    orders = allOrders.FindAll(o => o.CustomerId == customerId);
                }
                catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
            }, ct);

            var paymentsTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, ct); 
                    if (_failPaymentsCustomerIds.Contains(customerId))
                    {
                        throw new TimeoutException("Payments store timeout");
                    }
                    payments = await _fileStore.ReadAsync<PaymentRecord>("payments.json", ct) ?? new();
                }
                catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
            }, ct);

            var emailsTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(250, ct);
                    emails = await _fileStore.ReadAsync<EmailRecord>("emails.json", ct) ?? new();
                }
                catch (Exception ex) { lock (exceptions) { exceptions.Add(ex); } }
            }, ct);

            try
            {
                await Task.WhenAll(profileTask, ordersTask, paymentsTask, emailsTask);
            }
            catch
            {
            }

            stopwatch.Stop();

            var result = new DashboardResult
            {
                Customer = customer,
                Orders = orders,
                Payments = payments,
                Emails = emails
            };

            return (result, stopwatch.Elapsed, exceptions);
        }
    }
}