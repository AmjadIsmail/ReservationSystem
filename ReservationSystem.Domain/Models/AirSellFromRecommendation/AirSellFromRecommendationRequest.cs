using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AirSellFromRecommendation
{
    public class AirSellFromRecommendationRequest
    {
        public string? messageFunction { get; set; }
        public string? additionalMessageFunction { get; set; }
        public ItineraryDetails? outBound { get; set; }
        public ItineraryDetails? inBound { get; set; }

    }
    public class ItineraryDetails
    {
        public string? origin { get; set; }
        public string? destination { get; set; }
        public string? messageFunction { get; set; }
        public SegmentInformation? segmentInformation { get; set; }
    }

    public class SegmentInformation
    {
        public travelProductInformation? travelProductInformation { get; set; }
    }
    public class travelProductInformation
    {
        public string? departureDate { get; set; }
        public string? boarding_airport { get; set; }
        public string? off_airport { get; set; }
        public string? marketing_company { get; set; }
        public string? flight_number { get; set; }
        public string? booking_class { get; set; }
        public RelatedproductInformation? relatedproductInformation { get; set; }
    }
    public class RelatedproductInformation
    {
        public string? quantity { get; set; }
        public string? statusCode { get; set; }
    }
}
