using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Models
{
    public class Enums
    {
        public enum LoyaltyTier
        {
            Bronze,
            Silver,
            Gold
        }

        public enum OrderStatus
        {
            Pending,
            Validated,
            Rejected,
            PaymentFailed,
            Completed,
            Failed
        }

        public enum PaymentStatus
        {
            Captured,
            Declined,
            Refunded
        }
    }
}
