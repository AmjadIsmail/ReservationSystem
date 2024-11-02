using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class TravelerPricing
    {
        public string? travelerId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? fareOption { get; set; }
        public string? travelerType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? associatedAdultId { get; set; }
        public Price? price { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<FareDetailsBySegment>? fareDetailsBySegment { get; set; }
    }
}
