using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class FareDetailsBySegment
    {
        public string? segmentId { get; set; }
        public string? cabin { get; set; }
        public string? fareBasis { get; set; }
        public string? brandedFare { get; set; }
        public string? brandedFareLabel { get; set; }
        [JsonProperty("class")]
        public string? @class { get; set; }
        public IncludedCheckedBags? includedCheckedBags { get; set; }
        public List<Amenity>? amenities { get; set; }
    }
}
