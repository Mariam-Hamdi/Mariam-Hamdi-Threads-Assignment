using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class SyncCustomerRepository
    {

        private readonly JsonFileStore _fileStore;

        public SyncCustomerRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public Customer? GetByIdSync(int id)
        {
             SimulationHelper.SimulateDbQuerySync("CustomerRepository");

            var customers = _fileStore.ReadSync<Customer>("customers.json");

            return customers?.FirstOrDefault(c => c.Id == id);
        }
    }
}
