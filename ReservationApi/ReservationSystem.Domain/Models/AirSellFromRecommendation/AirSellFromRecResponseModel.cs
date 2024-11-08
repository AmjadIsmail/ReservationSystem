using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.AirSellFromRecommendation
{
    public class AirSellFromRecResponseModel
    {
        public HeaderSession? session { get; set; }
        public List<AirSellItineraryDetails> airSellResponse { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
    }
    public class AirSellItineraryDetails

    {
        public string? messageFunction { get; set; }
        public OriginDestination? originDestination { get; set; }
        public List<AirSellFlightDetails>? flightDetails { get; set; }
    }

    public class OriginDestination
    {
        public string? origin { get; set; }
        public string? destination { get; set; }
    }
    public class AirSellFlightDetails
    {
        public DateOnly? departureDate { get; set; }
        public TimeOnly? departureTime { get; set; }
        public DateOnly? arrivalDate { get; set; }
        public TimeOnly? arrivalTime { get; set; }
        public string? dateVariation { get; set; }
        public string? fromAirport { get; set; }
        public string? fromAirportName { get; set; }
        public string? toAirport { get; set; }
        public string? toAirportName { get; set; }
        public string? marketingCompany { get; set; }
        public string? marketingCompanyName { get; set; }
        public string? flightNumber { get; set; }
        public string? bookingClass { get; set; }
        public string? flightIndicator { get; set; }
        public string? specialSegment { get; set; }
        public LegDetails legdetails { get; set; }
        public string? departureTerminal { get; set; }
        public string? arrivalTerminal { get; set; }
        public string? statusCode { get; set; }
    }
    public class LegDetails
    {
        public string equipment { get; set; }
    }
}