using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;
using static Order_Processing_System.Models.Enums;

namespace Order_Processing_System.Services
{
   public class OrderRepository
    {
        private readonly JsonFileStore _fileStore;
        public OrderRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
      
        public async Task SaveAsync(Order order,CancellationToken ct)
        {
            await SimulationHelper.SimulateDbWriteAsync("OrderRepository", async () =>
            {
                var orders = await _fileStore.ReadAsync<Order>("orders.json", ct);
                var existingIndex = orders.FindIndex(o => o.Id == order.Id);
                if(existingIndex >= 0)
                {
                    orders[existingIndex] = order;
                }
                else
                {
                    orders.Add(order);
                }

                await _fileStore.WriteAsync("orders.json", orders, ct);


            },ct);
       
        }
         public async Task <List<Order>> GetAllAsync(CancellationToken ct=default)
        {
            await SimulationHelper.SimulateDbQueryAsync("OrderRepository", ct);
            var orders = await _fileStore.ReadAsync<Order>("orders.json", ct);
            return orders;
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus s,CancellationToken ct=default)
        {
            await SimulationHelper.SimulateDbQueryAsync("OrderRepository", ct);
            var orders = await _fileStore.ReadAsync<Order>("orders.json", ct);
            if (orders == null) return new List <Order> ();
           return orders.Where(o => o.Status == s).ToList();
        
        }
        // =========Sync ===========
        public void SaveSync(Order order)
        {
             SimulationHelper.SimulateDbWriteSync("OrderRepository",  () =>
            {
                var orders =  _fileStore.ReadSync<Order>("orders.json");
                var existingIndex = orders.FindIndex(o => o.Id == order.Id);
                if (existingIndex >= 0)
                {
                    orders[existingIndex] = order;
                }
                else
                {
                    orders.Add(order);
                }

                _fileStore.WriteSync("orders.json", orders);


            });

        }
        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            await SaveAsync(order, ct);
        }

    }
 }

