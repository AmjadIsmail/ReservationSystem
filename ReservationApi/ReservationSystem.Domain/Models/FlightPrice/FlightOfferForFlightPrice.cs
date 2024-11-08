using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FlightPrice
{
    public class FlightOfferForFlightPrice
    {
        public string? messageFunction { get; set; }
        public List<Itinerary>? itineraries { get; set; }
        public Price? price { get; set; }
        public PriceOption? pricingOptions { get; set; }
        public List<string>? validatingAirlineCodes { get; set; }
        public List<TravelerPricing>? travelerPricings { get; set; }
    }
}