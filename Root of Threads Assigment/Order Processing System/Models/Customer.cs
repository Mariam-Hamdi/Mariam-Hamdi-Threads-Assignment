using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Models
{
    public  class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // this property can never be null if no value is given ,it defualts to an empty string "" 
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public LoyaltyTier LoyaltyTier { get; set; }
    }
}
