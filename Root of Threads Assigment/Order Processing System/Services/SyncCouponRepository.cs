using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class SyncCouponRepository
    {
        private readonly JsonFileStore _fileStore;
        public SyncCouponRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public  Coupon? GetByCodeSync(string code)
        {
             SimulationHelper.SimulateDbQuerySync("CouponRepository");
            var coupons =  _fileStore.ReadSync<Coupon>("coupons.json");
            return coupons?.FirstOrDefault(coupons => String.Equals(coupons.Code, code, StringComparison.OrdinalIgnoreCase));
        }

    }
}
