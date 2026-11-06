using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order_Processing_System.Models;

namespace Order_Processing_System.Services
{
    public class InvoiceGenerator
    {
        public async Task GenerateAsync(Order order, CancellationToken ct = default)
        {
            await SimulationHelper.SimulateExternalApiAsync($"InvoiceGenerator-Order#{order.Id}", ct);

            order.TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        }
    }
}
