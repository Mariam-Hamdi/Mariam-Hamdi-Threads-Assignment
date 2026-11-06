using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class IncomingOrderRepository
    {
        private readonly JsonFileStore _fileStore;
        public IncomingOrderRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public async Task<List<Order>> GetAllAsync(CancellationToken ct=default)
        {
            await SimulationHelper.SimulateDbQueryAsync("IncomingOrderRepository", ct);

            var incomingOrders = await _fileStore.ReadAsync<Order>("incoming_orders.json", ct);
            if (incomingOrders == null) return new List<Order>();

            return incomingOrders;

        }
        }
}
