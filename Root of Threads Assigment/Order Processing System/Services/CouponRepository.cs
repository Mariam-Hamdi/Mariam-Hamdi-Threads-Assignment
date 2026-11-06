using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class CouponRepository
    {
        private readonly JsonFileStore _fileStore;
        public CouponRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public async Task<Coupon?> GetByCodeAsync(string code,CancellationToken ct)
        {
            await SimulationHelper.SimulateDbQueryAsync("CouponRepository", ct);
            var coupons = await _fileStore.ReadAsync<Coupon>("coupons.json",ct);
            return coupons?.FirstOrDefault(coupons => String.Equals(coupons.Code, code, StringComparison.OrdinalIgnoreCase));
        }

    }
}
