using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Availability
{
    public class AvailabilityRequest
    {
        public string? origin { get; set; }
        public string? destination { get; set; }
        public string? departureDate { get; set; }
        public string? returnDate { get; set; }
        public int? adults { get; set; }
        public int? children { get; set; }
        public int? infant { get; set; }
        public string? travelClass { get; set; }
        public string? includeAirlines { get; set; }
        public string? excludeAirlines { get; set; }
        public bool? nonStop { get; set; }
        public string? currencyCode { get; set; }
        public int? maxPrice { get; set; }
        public int? maxFlights { get; set; }
    }
}
