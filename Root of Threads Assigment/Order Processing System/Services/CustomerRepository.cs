using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class CustomerRepository
    {
        private readonly JsonFileStore _fileStore;

        public CustomerRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await SimulationHelper.SimulateDbQueryAsync("CustomerRepository", ct);

            var customers = await _fileStore.ReadAsync<Customer>("customers.json", ct);

            return customers?.FirstOrDefault(c => c.Id == id);
        }
    }
}