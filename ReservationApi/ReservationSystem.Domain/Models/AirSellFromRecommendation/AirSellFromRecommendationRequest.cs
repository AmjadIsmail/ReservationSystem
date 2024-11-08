

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AirSellFromRecommendation
{
    public class AirSellFromRecommendationRequest
    {
       // public HeaderSession sessionDetails { get; set; }
        public string? messageFunction { get; set; }
        public string? additionalMessageFunction { get; set; }
        public ItineraryDetails? outBound { get; set; }
        public ItineraryDetails? inBound { get; set; }

    }
    public class ItineraryDetails
    {
        public string? origin { get; set; }
        public string? destination { get; set; }
        public SegmentInformation? segmentInformation { get; set; }
    }

    public class SegmentInformation
    {
        public List<travelProductInformation>? travelProductInformation { get; set; }
    }
    public class travelProductInformation
    {
        public string? departureDate { get; set; }
        public string? fromAirport { get; set; }
        public string? toAirport { get; set; }
        public string? marketingCompany { get; set; }
        public string? flightNumber { get; set; }
        public string? bookingClass { get; set; }
        public RelatedproductInformation? relatedproductInformation { get; set; }
    }
    public class RelatedproductInformation
    {
        public string? quantity { get; set; }
        public string? statusCode { get; set; }
    }
}
