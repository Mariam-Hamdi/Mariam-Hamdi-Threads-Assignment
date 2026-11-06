using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class OrderValidator
    {
        private readonly CustomerRepository _customerRepo;
        private readonly ProductRepository _productRepo;
        private readonly CouponRepository _couponRepo;
        private readonly OrderRepository _orderRepo;

        public OrderValidator(
            CustomerRepository customerRepo,
            ProductRepository productRepo,
            CouponRepository couponRepo, OrderRepository orderRepo)
        {
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _couponRepo = couponRepo;
            _orderRepo = orderRepo;
        }
        public async Task<bool> ValidateOrderAsync(Order order, CancellationToken ct)
        {
            var customer = await _customerRepo.GetByIdAsync(order.CustomerId, ct);
            if (customer == null)
            {
                await FailOrderAsync(order,$" Customer #{order.CustomerId} not found",ct);
                return false;
            }
            if(!customer.IsActive)
            {
                await FailOrderAsync(order, "Customer is inactive", ct);
                return false;
            }
            foreach (var o in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(o.ProductId, ct);
                if (product == null)
                {
                    await FailOrderAsync(order, $"Product #{o.ProductId} not found", ct);
                    return false;
                }
                if (product.StockQuantity < o.Quantity)
                {
                    await FailOrderAsync(order, $"Product #{o.ProductId} does not have enough stock", ct);
                    return false;
                }
            }

                var copoun = await _couponRepo.GetByCodeAsync(order.CouponCode, ct);
                if (copoun == null)
                {
                  await  FailOrderAsync(order, $"Coupon code {order.CouponCode} not found", ct);
                    return false;
                }
                if (copoun.ExpiryDate < DateTime.Today)
                {
                   await FailOrderAsync(order, $"Coupon code {order.CouponCode} is expired", ct);
                    return false;
                }
                order.Status = OrderStatus.Validated;
                order.FailureReason = null;
                await _orderRepo.SaveAsync(order, ct);
                 return true;
        }
            public async Task FailOrderAsync(Order order,string reason,CancellationToken ct =default)
            {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = reason;
            await _orderRepo.SaveAsync(order, ct);

        }
         }
}
