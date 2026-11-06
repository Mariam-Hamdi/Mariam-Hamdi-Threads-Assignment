using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Models
{
    public class PaymentResult
    {
        public bool Success { get; set; }       
        public string? TransactionId { get; set; }  // this propert is optional.It is fine if it is null or missing
        public PaymentStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
