using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Models
{
   public class Report
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }     
        public int CompletedCount { get; set; }
        public int RejectedCount { get; set; }
        public int FailedCount { get; set; }
        public int PaymentFailedCount { get; set; }

        public string TopSellingProduct { get; set; } = string.Empty; // this property can never be null,if no value is given it defaults to empty string
        public int TopSellingQuantity { get; set; }   
        //Moneys always is decimal
        public decimal AverageOrderValue { get; set; }   
        public DateTime GeneratedAt { get; set; }
    }
}
