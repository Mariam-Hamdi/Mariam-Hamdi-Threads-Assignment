using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class OrderProcessor
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderPricer _orderPricer;
        private readonly IPaymentGateway _paymentGateway;
        private readonly OrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly JsonFileStore _fileStore;

        public OrderProcessor(
            CustomerRepository customerRepository,
            ProductRepository productRepository,
            OrderPricer orderPricer,
            IPaymentGateway paymentGateway,
            OrderRepository orderRepository,
            IEmailService emailService,
            JsonFileStore fileStore)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderPricer = orderPricer;
            _paymentGateway = paymentGateway;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _fileStore = fileStore;
        }

        public async Task<bool> ProcessOrderAsync(Order order, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            int threadId = Environment.CurrentManagedThreadId;

            try
            {
                var customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);
                if (customer == null || !customer.IsActive)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = customer == null ? $"Customer #{order.CustomerId} not found" : "Customer is inactive";
                    await _orderRepository.SaveAsync(order, ct);
                    LogStep(threadId, order.Id, "1/6 Customer", "Rejected", stopwatch.ElapsedMilliseconds);
                    return false;
                }

                var products = await _fileStore.ReadAsync<Product>("products.json", ct) ?? [];
                foreach (var item in order.Items)
                {
                    var product = products.Find(p => p.Id == item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        order.Status = OrderStatus.Rejected;
                        order.FailureReason = $"Insufficient stock for product #{item.ProductId}";
                        await _orderRepository.SaveAsync(order, ct);
                        LogStep(threadId, order.Id, "2/6 Inventory", "Insufficient stock", stopwatch.ElapsedMilliseconds);
                        return false;
                    }
                    product.StockQuantity -= item.Quantity;
                }
                await _fileStore.WriteAsync("products.json", products, ct);

                try
                {
                    await _orderPricer.PriceOrderAsync(order, ct); 
                }
                catch (Exception ex)
                {
                    foreach (var item in order.Items)
                    {
                        var product = products.Find(p => p.Id == item.ProductId);
                        if (product != null) product.StockQuantity += item.Quantity;
                    }
                    await _fileStore.WriteAsync("products.json", products, ct);

                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = ex.Message;
                    await _orderRepository.SaveAsync(order, ct);
                    LogStep(threadId, order.Id, "3/6 Pricing", $"Rejected ({ex.Message})", stopwatch.ElapsedMilliseconds);
                    return false;
                }

                string transactionId;
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

                    transactionId = await _paymentGateway.ProcessPaymentAsync(order.Id, order.TotalAmount, timeoutCts.Token);
                }
                catch (Exception ex)
                {
                    foreach (var item in order.Items)
                    {
                        var product = products.Find(p => p.Id == item.ProductId);
                        if (product != null) product.StockQuantity += item.Quantity;
                    }
                    await _fileStore.WriteAsync("products.json", products, ct);

                    order.Status = OrderStatus.PaymentFailed;
                    order.FailureReason = ex is TimeoutException ? "Payment gateway timeout (3s)." : ex.Message;
                    await _orderRepository.SaveAsync(order, ct);
                    LogStep(threadId, order.Id, "4/6 Payment", $"Failed ({order.FailureReason})", stopwatch.ElapsedMilliseconds);
                    return false;
                }

                order.TransactionId = transactionId;
                order.Status = OrderStatus.Completed;

                try
                {
                    await _orderRepository.SaveAsync(order, ct);
                }
                catch (Exception)
                {
                    await _paymentGateway.RefundAsync(transactionId, ct);
                    foreach (var item in order.Items)
                    {
                        var product = products.Find(p => p.Id == item.ProductId);
                        if (product != null) product.StockQuantity += item.Quantity;
                    }
                    await _fileStore.WriteAsync("products.json", products, ct);

                    order.Status = OrderStatus.Failed;
                    order.FailureReason = $"Database save failed after payment; payment {transactionId} refunded";
                    await _orderRepository.SaveAsync(order, ct);
                    LogStep(threadId, order.Id, "5/6 Save", "FAILED & Refunded", stopwatch.ElapsedMilliseconds);
                    return false;
                }

                LogStep(threadId, order.Id, "4/6 Payment", $"OK ({transactionId})", stopwatch.ElapsedMilliseconds);

                await _emailService.SendConfirmationWithRetryAsync(order, customer, ct);

                return true;
            }
            catch (Exception ex)
            {
                order.Status = OrderStatus.Failed;
                order.FailureReason = ex.Message;
                await _orderRepository.SaveAsync(order, ct);
                return false;
            }
        }

        private static void LogStep(int threadId, int orderId, string step, string status, long elapsedMs)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Thread {threadId}] Order #{orderId} -> Step {step} ... {status} in {elapsedMs} ms");
        }
    }
}