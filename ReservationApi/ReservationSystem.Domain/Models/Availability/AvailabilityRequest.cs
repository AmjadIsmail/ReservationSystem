using ReservationSystem.Domain.Models.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace ReservationSystem.Domain.Models.Availability
{
    public class AvailabilityRequest
    {
        public string? origin { get; set; }
        public string? destination { get; set; }

        [DateCheck(360)]
        public string? departureDate { get; set; }
        [DateCheck(360)]
        public string? returnDate { get; set; }
        [PassengerCheck]
        public int? adults { get; set; }
        public int? children { get; set; }
        [PassengerCheck]
        public int? infant { get; set; }
        public string? cabinClass { get; set; }
        public string? flightType { get; set; } // direct / indirect
        public string? includeAirlines { get; set; }
        public string? excludeAirlines { get; set; }
        public bool? nonStop { get; set; }
        public string? currencyCode { get; set; }
        public int? maxPrice { get; set; }
        public int? maxFlights { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
