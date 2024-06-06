using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class AvailabilityResult
    {
        public Meta meta { get; set; }
        public List<FlightOffer> data { get; set; }
        public Dictionaries dictionaries { get; set; }
    }
    public class Dictionaries
    {
        public Dictionary<string, Location> locations { get; set; }
        public Dictionary<string, string> aircraft { get; set; }
        public Dictionary<string, string> currencies { get; set; }
        public Dictionary<string, string> carriers { get; set; }
    }

    public class Location
    {
        public string cityCode { get; set; }
        public string countryCode { get; set; }
    }
    public class Meta
    {
        public int count { get; set; }
        public Links links { get; set; }
    }
    public class Links
    {
        public string self { get; set; }
    }

    public class Departure
    {
        public string iataCode { get; set; }
        public string terminal { get; set; }
        public DateTime at { get; set; }
    }

    public class Arrival
    {
        public string iataCode { get; set; }
        public string terminal { get; set; }
        public DateTime at { get; set; }
    }

    public class Aircraft
    {
        public string code { get; set; }
    }
    public class Operating
    {
        public string carrierCode { get; set; }
    }

    public class Segment
    {
        public Departure departure { get; set; }
        public Arrival arrival { get; set; }
        public string carrierCode { get; set; }
        public string number { get; set; }
        public Aircraft aircraft { get; set; }
        public Operating operating { get; set; }
        public string duration { get; set; }
        public string id { get; set; }
        public int numberOfStops { get; set; }
        public bool blacklistedInEU { get; set; }
    }
    public class Itinerary
    {
        public string duration { get; set; }
        public List<Segment> segments { get; set; }
    }

    public class Price
    {
        public string currency { get; set; }
        public string total { get; set; }
        public string base1 { get; set; }
        public List<Fee> fees { get; set; }
       public string grandTotal { get; set; }
}
public class Fee
{
    public string amount { get; set; }
    public string type { get; set; }
}

public class PriceOption
{
    public List<string> fareType { get; set; }
    public bool includedCheckedBagsOnly { get; set; }
}
public class TravelerPricing
{
    public string travelerId { get; set; }
    public string fareOption { get; set; }
    public string travelerType { get; set; }
    public Price price { get; set; }
    public List<FareDetailsBySegment> fareDetailsBySegment { get; set; }
}

public class FareDetailsBySegment
{
    public string segmentId { get; set; }
    public string cabin { get; set; }
    public string fareBasis { get; set; }
    public string brandedFare { get; set; }
    public string brandedFareLabel { get; set; }
    public string @class { get; set; }
    public IncludedCheckedBags includedCheckedBags { get; set; }
    public List<Amenity> amenities { get; set; }
}

public class IncludedCheckedBags
{
    public int quantity { get; set; }
}

public class Amenity
{
    public string description { get; set; }
    public bool isChargeable { get; set; }
    public string amenityType { get; set; }
    public AmenityProvider amenityProvider { get; set; }
}

public class AmenityProvider
{
    public string name { get; set; }
}

public class FlightOffer
{
    public string type { get; set; }
    public string id { get; set; }
    public string source { get; set; }
    public bool instantTicketingRequired { get; set; }
    public bool nonHomogeneous { get; set; }
    public bool oneWay { get; set; }
    public string lastTicketingDate { get; set; }
    public string lastTicketingDateTime { get; set; }
    public int numberOfBookableSeats { get; set; }
    public List<Itinerary> itineraries { get; set; }
    public Price price { get; set; }
    public PriceOption pricingOptions { get; set; }
    public List<string> validatingAirlineCodes { get; set; }
    public List<TravelerPricing> travelerPricings { get; set; }
}

public class Root
{
    public Meta meta { get; set; }
    public List<FlightOffer> data { get; set; }
    public Dictionaries dictionaries { get; set; }
}

}
