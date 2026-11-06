using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Processing_System.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // this property can never be null , if no value is given , it defaults to emtpy string""
        public decimal Price { get; set; } // Money is always decimal
        public int StockQuantity { get; set; }
        public decimal WeightKg { get; set; }
    }
}
