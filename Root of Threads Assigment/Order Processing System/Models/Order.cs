using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Models
{
   public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public List<OrderItem> Items { get; set; } = new(); 

        public string? CouponCode { get; set; } // 

        // Money is always decimal
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalWeightKg { get; set; }
        public int DiscountPercent { get; set; } 
        // // these properties are optional, it is totally fine if they are null or missing 
        public string? FailureReason { get; set; }
        public string? StatusMessage { get; set; } 
        public string? TransactionId { get; set; }
    }
}
