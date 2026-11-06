using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class SyncProductRepository
    {
        private readonly JsonFileStore _fileStore;

        public SyncProductRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public Product? GetByIdSync(int id)
        {
            SimulationHelper.SimulateDbQuerySync("ProductRepository");

            var products =  _fileStore.ReadSync<Product>("products.json");
            return products?.FirstOrDefault(p => p.Id == id);
        }

        public bool ReserveStockSync(int productId, int qt)
        {
            bool success = false;
             SimulationHelper.SimulateDbWriteSync("ProductRepository",() =>
            {
                var products = _fileStore.ReadSync<Product>("products.json");
                var product = products.FirstOrDefault(product => product.Id == productId);
                if (product == null || product.StockQuantity < qt)
                { success = false; return; }
                product.StockQuantity -= qt;
                success = true;

                _fileStore.WriteSync("products.json", products);




            });
            return success;
        }
    }
}