using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PricePnr
{
    public class PricePnrRequest
    {
        public HeaderSession? sessionDetails { get; set; }
        public string? pricingOptionKey { get; set; }
        public string? carrierCode { get; set; }
    }
}
