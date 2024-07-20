using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReservationSystem.Domain.Models
{
    public class Price
    {
        public string? currency { get; set; }
        public string? total { get; set; }
        [JsonProperty("base")]
        public string? base_amount { get; set; }
        public List<Taxes>? taxes { get; set; }
        public List<Fee>? fees { get; set; }
        public string? grandTotal { get; set; }
        public string? billingCurrency { get; set; }
        public string? refundableTaxes { get; set; }
        public decimal? markup { get; set; }
    }
}
