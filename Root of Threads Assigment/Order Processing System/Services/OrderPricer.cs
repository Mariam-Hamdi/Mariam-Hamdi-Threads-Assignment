using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
   public class OrderPricer
    {
        private readonly  HashSet<int> _failShippingOrderIds;
        private readonly ProductRepository _productRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly CouponRepository _couponRepository;

       

        public OrderPricer(IEnumerable<int> failShippingOrderIds, ProductRepository productRepository, 
            CustomerRepository customerRepository,
            CouponRepository couponRepository)
        {
            _failShippingOrderIds = new HashSet<int> (failShippingOrderIds) ;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _couponRepository = couponRepository;
        }
        public async Task<Order> PriceOrderAsync(Order order, CancellationToken ct = default)
        {
            await CalculateSubTotalAsync(order, ct);

            var discountTask = GetDiscountAsync(order, ct);
            var shippingTask = CalculateShippingCostAsync(order, ct);

            await Task.WhenAll(discountTask, shippingTask);

            order.TotalAmount = Math.Round(order.SubTotal - order.Discount + order.ShippingCost, 2, MidpointRounding.AwayFromZero);

            return order;
        }

        public async Task<Order> PriceOrderSequentialAsync(Order order, CancellationToken ct = default)
        {
            await CalculateSubTotalAsync(order, ct);
            await GetDiscountAsync(order, ct);
            await CalculateShippingCostAsync(order, ct);

            order.TotalAmount = Math.Round(order.SubTotal - order.Discount + order.ShippingCost, 2, MidpointRounding.AwayFromZero);
            return order;
        }
        public async Task CalculateSubTotalAsync(Order order, CancellationToken ct)
        {
            decimal subTotal = 0;
            decimal totalWeight = 0;
            int totalQty = 0;
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId,ct);
                if (product != null)
                {
                    item.UnitPrice = product.Price;
                    item.WeightKg = product.WeightKg;
                }
                    subTotal += item.UnitPrice * item.Quantity;
                    totalQty += item.Quantity;
                    totalWeight += item.WeightKg * item.Quantity;
                    }
            order.SubTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
                order.TotalQuantity = totalQty;
                order.TotalWeightKg = totalWeight;
        }
       public async Task<decimal> GetDiscountAsync(Order order,CancellationToken ct=default)
        {
            var loyaltyPercent = 0;
            var couponPercent = 0;
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId,ct);  
        
            if(customer !=null)
            {
                loyaltyPercent = customer.LoyaltyTier switch
                {
                    LoyaltyTier.Silver => 5,
                    LoyaltyTier.Gold => 10,
                    _ =>0 
                };
            }
            if (!string.IsNullOrEmpty(order.CouponCode))
            {
                var copoun = await _couponRepository.GetByCodeAsync(order.CouponCode,ct);
                if (copoun != null)
                {
                    couponPercent = copoun.DiscountPercent;
                }
            }
            int totalDiscountPercent = loyaltyPercent + couponPercent;
            if (totalDiscountPercent > 30) totalDiscountPercent = 30;

            order.DiscountPercent = totalDiscountPercent;
            decimal discountVal = order.SubTotal * totalDiscountPercent / 100;
            order.Discount = Math.Round(discountVal, 2, MidpointRounding.AwayFromZero);
            return order.Discount;
        }

        public async Task CalculateShippingCostAsync(Order order, CancellationToken ct=default)
        {
            await SimulationHelper.SimulateExternalApiAsync($"ShippingAPI-Order#{order.Id}", ct);
            if(_failShippingOrderIds.Contains(order.Id))
                throw new ShippingProviderUnavailableException("Shipping provider unavailable");
            decimal shippingCost = 0;
            if (order.SubTotal < 1000)
            {
                shippingCost = 5.00m + (1.50m * order.TotalQuantity) + (2.00m * (decimal)order.TotalWeightKg);
            }
            else
            {
                shippingCost = 0.00m;
            }

            order.ShippingCost = Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
        }
    }
}
