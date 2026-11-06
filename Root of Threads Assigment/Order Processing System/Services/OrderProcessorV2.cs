using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class OrderProcessorV2
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderPricer _orderPricer;
        private readonly InvoiceGenerator _invoiceGenerator;
        private readonly OrderRepository _orderRepository;

        public OrderProcessorV2(
            ProductRepository productRepository,
            OrderPricer orderPricer,
            InvoiceGenerator invoiceGenerator,
            OrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _orderPricer = orderPricer;
            _invoiceGenerator = invoiceGenerator;
            _orderRepository = orderRepository;
        }

        public async Task<bool> ProcessValidatedOrderAsync(Order order, CancellationToken ct = default)
        {
            var reservedItems = new List<(int ProductId, int Quantity)>();

            try
            {
                foreach (var item in order.Items)
                {
                    bool reserved = await _productRepository.ReserveStockAsync(item.ProductId, item.Quantity, ct);
                    if (!reserved)
                    {
                        foreach (var ri in reservedItems)
                        {
                            await _productRepository.ReleaseStockAsync(ri.ProductId, ri.Quantity, ct);
                        }

                        order.Status = OrderStatus.Rejected;
                        order.StatusMessage = $"Insufficient stock for product #{item.ProductId}";
                        await _orderRepository.UpdateAsync(order, ct); // حفظ التغيير
                        return false;
                    }
                    reservedItems.Add((item.ProductId, item.Quantity));
                }

                // 2. حساب السعر (التسعير والشحن والخصم)
                await _orderPricer.PriceOrderAsync(order, ct);

                // 3. توليد الفاتورة
                await _invoiceGenerator.GenerateAsync(order, ct);

                // 4. نجاح تام! تحويل الأوردر لـ Completed
                order.Status = OrderStatus.Completed;
                await _orderRepository.UpdateAsync(order, ct);
                return true;
            }
            catch (Exception ex)
            {
                // لو حصلت مشكلة غير متوقعة (زي عطل الشحن في أوردر #7)
                foreach (var ri in reservedItems)
                {
                    await _productRepository.ReleaseStockAsync(ri.ProductId, ri.Quantity, ct);
                }

                order.Status = OrderStatus.Failed;
                order.StatusMessage = ex.Message;
                await _orderRepository.UpdateAsync(order, ct);

                throw;
            }
        }
    }
}