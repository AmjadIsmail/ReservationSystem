using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Segment
    {
        public Departure? departure { get; set; }
        public Arrival? arrival { get; set; }
        public string? marketingCarrierCode { get; set; }
        public string? marketingCarrierName { get; set; }
        public string? number { get; set; }
        public Aircraft? aircraft { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Operating? operating { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? duration { get; set; }
        public int? id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? segmentRef { get; set; }
        public int? numberOfStops { get; set; }       
        public BaggageAllowance? baggageAllowence { get; set; }
        public string? cabinClass { get; set; }
        public string? bookingClass {get;set;}
        public string? avlStatus { get; set; }
        public string? fareBasis { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? breakPoint { get; set; }
        public string? cabinStatus { get; set; }
        public string? rateClass { get; set; }
    }

    public class BaggageAllowance
    {
        public string? free_allowance { get; set; }
        public string? quantity_code { get; set; }
    }
}
