using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class ProductRepository
    {
        private readonly JsonFileStore _fileStore;
        private readonly SemaphoreSlim _semaphore =new(1,1);
        public ProductRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await SimulationHelper.SimulateDbQueryAsync("ProductRepository", ct);

            var products = await _fileStore.ReadAsync<Product>("products.json", ct);
            return products?.FirstOrDefault(p => p.Id == id); 
        }
        public async Task<bool> ReleaseStockAsync(int productId, int qt, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                bool success = false;
                await SimulationHelper.SimulateDbWriteAsync("ProductRepository", async () =>
                {
                    var products = await _fileStore.ReadAsync<Product>("products.json", ct);
                    var product = products.FirstOrDefault(p => p.Id == productId);
                    if (product == null)
                    {
                        success = false;
                        return;
                    }
                    product.StockQuantity += qt; 
                    success = true;

                    await _fileStore.WriteAsync("products.json", products, ct);
                }, ct);

                return success;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<bool> ReserveStockAsync(int productId, int qt, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                bool success = false;
                await SimulationHelper.SimulateDbWriteAsync("ProductRepository", async () =>
               {
                   var products = await _fileStore.ReadAsync<Product>("products.json", ct);
                   var product = products.FirstOrDefault(product => product.Id == productId);
                   if (product == null || product.StockQuantity < qt)
                   { success = false; return; }
                   product.StockQuantity -= qt;
                   success = true;

                   await _fileStore.WriteAsync("products.json", products, ct);




               }, ct);
                return success;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<bool> ReserveStockUnsafeAsync(int productId, int qt, CancellationToken ct = default)
        {
            await   Task.Delay(200, ct);
            bool success = false;
            await SimulationHelper.SimulateDbWriteAsync("ProductRepository", async () =>
            {
                var products = await _fileStore.ReadAsync<Product>("products.json", ct);
                var product = products.FirstOrDefault(product => product.Id == productId);
                await Task.Delay(100, ct);

                if (product == null || product.StockQuantity < qt)
                { success = false; return; }
                await Task.Delay(400, ct);

                product.StockQuantity -= qt;
                success = true;

                await _fileStore.WriteAsync("products.json", products, ct);




            }, ct);
            return success;
        }


    }
}