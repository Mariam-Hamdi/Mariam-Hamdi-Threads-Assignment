using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Models
{
    public class Coupon
    {
        public string Code { get; set; } = string.Empty; // this propert can never equall null , if no value is given , it defaults to empty string""
        public int DiscountPercent { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
