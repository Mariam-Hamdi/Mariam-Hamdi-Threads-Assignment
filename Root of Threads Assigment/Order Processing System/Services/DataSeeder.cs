using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Order_Processing_System.Models;
using System.Text.Json.Serialization;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
    public class DataSeeder
    { 
        private readonly JsonFileStore _fileStore;

        public DataSeeder(JsonFileStore fileStore) { _fileStore = fileStore; }
        public async Task ResetAsync(CancellationToken ct)
        {
            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            if (Directory.Exists(dataDirectory))
                Directory.Delete(dataDirectory,recursive:true);
            Directory.CreateDirectory(dataDirectory);
            var customers = new List<Customer>
            {
                new() { Id = 1, Name = "Ahmed Ali", Email = "ahmed.ali@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Gold },
                new() { Id = 2, Name = "Sara Mohamed", Email = "sara.mohamed@example.com", IsActive = true, LoyaltyTier =LoyaltyTier.Silver },
                new() { Id = 3, Name = "Omar Hassan", Email = "omar.hassan@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze },
                new() { Id = 4, Name = "Mona Ibrahim", Email = "mona.ibrahim@example.com", IsActive = false, LoyaltyTier = LoyaltyTier.Silver },
                new() { Id = 5, Name = "Youssef Adel", Email = "youssef.adel@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Gold },
                new() { Id = 6, Name = "Nour Khaled", Email = "nour.khaled@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze },
                new() { Id = 7, Name = "Karim Samir", Email = "karim.samir@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Silver },
                new() { Id = 8, Name = "Laila Mahmoud", Email = "laila.mahmoud@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Gold },
                new() { Id = 9, Name = "Hassan Tarek", Email = "hassan.tarek@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze },
                new() { Id = 10, Name = "Dina Fathy", Email = "dina.fathy@example.com", IsActive = true, LoyaltyTier = LoyaltyTier.Silver }
            };
            await _fileStore.WriteAsync("customers.json", customers, ct);
            Console.WriteLine($"[Seeder] customers.json -> {customers.Count} records.");

            var products = new List<Product>
            {
                new() { Id = 1, Name = "Laptop", Price = 1200.00m, StockQuantity = 10, WeightKg = 2.2m },
                new() { Id = 2, Name = "Smartphone", Price = 800.00m, StockQuantity = 15, WeightKg = 0.4m },
                new() { Id = 3, Name = "Wireless Mouse", Price = 25.00m, StockQuantity = 2, WeightKg = 0.1m },
                new() { Id = 4, Name = "Mechanical Keyboard", Price = 90.00m, StockQuantity = 1, WeightKg = 0.9m },
                new() { Id = 5, Name = "USB-C Hub", Price = 45.00m, StockQuantity = 30, WeightKg = 0.2m },
                new() { Id = 6, Name = "Headphones", Price = 150.00m, StockQuantity = 12, WeightKg = 0.3m },
                new() { Id = 7, Name = "27\" Monitor", Price = 350.00m, StockQuantity = 6, WeightKg = 5.0m },
                new() { Id = 8, Name = "Webcam", Price = 70.00m, StockQuantity = 20, WeightKg = 0.2m }
            };
            await _fileStore.WriteAsync("products.json", products, ct);
            Console.WriteLine($"[Seeder] products.json -> {products.Count} records.");

            var coupons = new List<Coupon>
            {
                new() { Code = "SAVE10", DiscountPercent = 10, ExpiryDate = DateTime.Today.AddDays(180) },
                new() { Code = "WELCOME15", DiscountPercent = 15, ExpiryDate = DateTime.Today.AddDays(90) },
                new() { Code = "FLASH20", DiscountPercent = 20, ExpiryDate = DateTime.Today.AddDays(7) },
                new() { Code = "OLD20", DiscountPercent = 20, ExpiryDate = DateTime.Today.AddDays(-30) } // منتهي الصلاحية
            };
            await _fileStore.WriteAsync("coupons.json", coupons, ct);
            Console.WriteLine($"[Seeder] coupons.json -> {coupons.Count} records.");

            var orders = new List<Order>
            {
                new() { Id = 1, CustomerId = 1, Items = [new() { ProductId = 1, Quantity = 1, UnitPrice = 0 }, new() { ProductId = 3, Quantity = 1, UnitPrice = 0 }], CouponCode = "SAVE10", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 2, CustomerId = 2, Items = [new() { ProductId = 2, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 3, CustomerId = 3, Items = [new() { ProductId = 5, Quantity = 2, UnitPrice = 0 }, new() { ProductId = 8, Quantity = 1, UnitPrice = 0 }], CouponCode = "WELCOME15", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 4, CustomerId = 4, Items = [new() { ProductId = 6, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 5, CustomerId = 5, Items = [new() { ProductId = 7, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 6, CustomerId = 6, Items = [new() { ProductId = 5, Quantity = 1, UnitPrice = 0 }], CouponCode = "FLASH20", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 7, CustomerId = 7, Items = [new() { ProductId = 6, Quantity = 1, UnitPrice = 0 }, new() { ProductId = 8, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 8, CustomerId = 8, Items = [new() { ProductId = 1, Quantity = 1, UnitPrice = 0 }], CouponCode = "SAVE10", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 9, CustomerId = 9, Items = [new() { ProductId = 3, Quantity = 5, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 10, CustomerId = 10, Items = [new() { ProductId = 4, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 11, CustomerId = 1, Items = [new() { ProductId = 4, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 12, CustomerId = 2, Items = [new() { ProductId = 2, Quantity = 1, UnitPrice = 0 }], CouponCode = "OLD20", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 13, CustomerId = 3, Items = [new() { ProductId = 6, Quantity = 2, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 14, CustomerId = 5, Items = [new() { ProductId = 8, Quantity = 2, UnitPrice = 0 }, new() { ProductId = 5, Quantity = 1, UnitPrice = 0 }], CouponCode = "WELCOME15", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 15, CustomerId = 99, Items = [new() { ProductId = 2, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 16, CustomerId = 6, Items = [new() { ProductId = 7, Quantity = 1, UnitPrice = 0 }, new() { ProductId = 5, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 17, CustomerId = 7, Items = [new() { ProductId = 2, Quantity = 1, UnitPrice = 0 }], CouponCode = "SAVE10", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 18, CustomerId = 8, Items = [new() { ProductId = 6, Quantity = 1, UnitPrice = 0 }], CouponCode = "FAKE50", Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 19, CustomerId = 9, Items = [new() { ProductId = 8, Quantity = 1, UnitPrice = 0 }], Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new() { Id = 20, CustomerId = 10, Items = [new() { ProductId = 1, Quantity = 1, UnitPrice = 0 }, new() { ProductId = 6, Quantity = 1, UnitPrice = 0 }], CouponCode = "FLASH20", Status = OrderStatus.Pending, CreatedAt = DateTime.Now }
            };
            await _fileStore.WriteAsync("incoming_orders.json", orders, ct);
            Console.WriteLine($"[Seeder] incoming_orders.json -> {orders.Count} records.");

        }
    }
}
