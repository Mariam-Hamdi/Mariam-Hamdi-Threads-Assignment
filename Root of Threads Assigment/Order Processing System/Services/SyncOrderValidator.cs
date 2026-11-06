using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class SyncOrderValidator
    {


        private readonly SyncCustomerRepository _customerRepo;
        private readonly SyncProductRepository _productRepo;
        private readonly SyncCouponRepository _couponRepo;
        private readonly OrderRepository _orderRepo;
        public SyncOrderValidator(
            SyncCustomerRepository customerRepo,
            SyncProductRepository productRepo,
            SyncCouponRepository couponRepo,
            OrderRepository orderRepo)
        {
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _couponRepo = couponRepo;
            _orderRepo = orderRepo;
        }

        public bool ValidateOrder(Order order)
        {
            var customer = _customerRepo.GetByIdSync(order.CustomerId);
            if (customer == null)
            {
                FailOrder(order, $"Customer #{order.CustomerId} not found");
                return false;
            }

            if (!customer.IsActive)
            {
                FailOrder(order, "Customer is inactive");
                return false;
            }

            foreach (var o in order.Items)
            {
                var product = _productRepo.GetByIdSync(o.ProductId);
                if (product == null)
                {
                    FailOrder(order, $"Product #{o.ProductId} not found");
                    return false;
                }

                if (product.StockQuantity < o.Quantity)
                {
                    FailOrder(order, $"Insufficient stock for '{product.Name}'");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(order.CouponCode))
            {
                var coupon = _couponRepo.GetByCodeSync(order.CouponCode);
                if (coupon == null)
                {
                    FailOrder(order, $"Coupon '{order.CouponCode}' does not exist");
                    return false;
                }

                if (coupon.ExpiryDate < DateTime.Today)
                {
                    FailOrder(order, $"Coupon '{order.CouponCode}' has expired");
                    return false;
                }
            }

            order.Status = OrderStatus.Validated;
            order.FailureReason = null;

            _orderRepo.SaveSync(order);
            return true;
        }

        private void FailOrder(Order order, string reason)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = reason;
            _orderRepo.SaveSync(order);
        }

   
        }
}
