using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Payment
{
    public class PaymentRequest
    {
        public string? OrderId { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; } = "GBP";
        public string? Language { get; set; } = "en_US";
    }
}
