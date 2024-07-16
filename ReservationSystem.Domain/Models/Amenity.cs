using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Amenity
    {
        public string? description { get; set; }
        public bool? isChargeable { get; set; }
        public string? amenityType { get; set; }
        public AmenityProvider? amenityProvider { get; set; }
    }
}
