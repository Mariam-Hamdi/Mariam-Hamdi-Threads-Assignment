using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Models
{
    public class PaymentTransaction
    {
        public int OrderId { get; set; }
        //Money is always decimal
        public decimal Amount { get; set; } 
        public PaymentStatus Status { get; set; }
        
        public string? TransactionId { get; set; } // this property is optonal.It is fine if it is null or missing
        public DateTime CreatedAt { get; set; }
    }
}
