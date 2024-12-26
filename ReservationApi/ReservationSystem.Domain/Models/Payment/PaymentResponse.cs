using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Payment
{
    public class PaymentResponse
    {
        public string? Url { get; set; }

        public Dictionary<string, string>? Parameters { get; set; }
        public string? Error { get; set; }
        public string? BookingRefNo { get; set; }
    }
}
