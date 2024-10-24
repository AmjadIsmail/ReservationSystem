using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AddPnrMulti
{
    public class AddPnrMultiResponse
    {
        public HeaderSession? session { get; set; }       
        public AmadeusResponseError? amadeusError { get; set; }
        public AddPnrMultiDetails addPnrMultiDetails { get; set; }
    }

    public class AddPnrMultiDetails
    {
        public SecurityInformation? securityInformation { get; set; }
        public PnrHeaderTag? pnrHeader { get; set; }
        public SbrUpdatorPosDetails? SbrUpdatorPosDetails { get; set; }
        public List<TravellerInfo>? travellerInfo { get; set; }
        public OriginDestinationDetails? destinationDetails { get; set; }
        public object elementsMaster { get; set; }
    }
    public class OriginDestinationDetails
    {
       public List<ItineraryInfo> itineraryInfos { get; set; }
    }
    public class ItineraryInfo
    {
        public ElementManagementItinerary? elementManagementItinerary { get; set; }
        public TravelProduct? travelProduct { get; set; }
        public FlightDetails? flightDetails { get; set; }
        public CabinDetails? cabinDetails { get; set; }
        public Sectiondetails sectiondetails { get; set; }
        public LegInfo leginfo { get; set; }
    }
    public class ElementManagementItinerary
    {
        public Reference? Reference { get; set; }
        public string? segmentName { get; set; }
        public string? lineNumber { get; set; }
    }
    public class DataElementsMaster
    {
        public List<DataElementsIndiv>? dataElementsIndivs { get; set; }
    }
    public class DataElementsIndiv
    {
        public ElementManagementData? elementManagement { get; set; }
        public OtherDataFreetext? otherDataFreetext { get; set; }
    }
    public class ElementManagementData
    {
        public string? qualifier { get; set; }
        public string? number { get; set; }
        public string? segmentName { get; set; }
        public string? lineNumber { get; set; }
    }
    public class OtherDataFreetext
    {
        public string?  subjectQualifier { get; set; }
        public string? type { get; set; }
        public string? longFreetext { get; set; }
    }
    public class FlightDetails
    {
        public string? equipment { get; set; }
        public string? numOfStops { get; set; }
        public string? duration { get; set; }
        public string? weekDay { get; set; }
        public string? departTerminal { get; set; }
        public string? arrivalTerminal { get; set; } 
        public string? flightLegMileage { get; set; }
    }
    public class CabinDetails
    {
        public string? classDesignator { get; set; }
    }
    public class LegInfo
    {
        public string? departureDate { get; set; }
        public string? departureTime { get; set; }
        public string? arrivalDate { get; set; }
        public string? arrivalTime { get; set; }
        public string? fromAirport { get; set; }
        public string? toAirport { get; set; }
    }
    public class Sectiondetails
    {
        public string? section { get; set; }
    }
    public class TravelProduct
    {
        public string? depDate { get; set; }
        public string? depTime { get; set; }
        public string? arrDate { get; set; }
        public string? arrTime { get; set; }
        public string? dayChangeIndicator { get; set; }
        public string? fromCity { get; set; }
        public string?  toCity { get; set; }
        public string?  company { get; set; }
        public string?  flightNumber { get; set; }
        public string?  cabinClass { get; set; }
        public string? typeDetails { get; set; }
    }
    public class SecurityInformation
    {
        public string? typeOfPnr { get; set; }
        public string? pnr { get; set; }
        public string? iataCode { get; set; }
        public string? OfficeId { get; set; }
        public string? cityCode { get; set; }
    }
    public class PnrHeaderTag
    {
        public string? indicator { get; set; }
    }
    public class SbrUpdatorPosDetails
    {
        public string? originatorId { get; set; }
        public string? inHouseIdentification1 { get; set; }
        public string? originatorTypeCode { get; set; }
        public SbrSystemDetails? sbrSystemDetails { get; set; }
        public SbrPreferences? sbrPreferences { get; set; }
    }
    public class SbrSystemDetails
    {
        public string? companyId { get; set; }
        public string? locationId { get; set; }
    }
    public class SbrPreferences
    {
        public string? codedCountry { get; set; }
    }

    public class TravellerInfo
    {
        public ElementManagementPassenger? passengerElement { get; set; }
        public PassengerData? passengerData { get; set; }
        public List<EnhancedPassengerData>? enhancedPassengerData { get; set; }
    }
    public class ElementManagementPassenger
    {
        public Reference? reference { get; set; }
        public string? segmentName { get; set; }
        public string? lineNumber { get; set; }
    }
    public class Reference
    {
        public string? qualifier { get; set; }
        public string? number { get; set; }
    }
    public class PassengerData
    {
        public Traveller? traveler { get; set; }
        public Passenger? passenger { get; set; }
       
        
    }
    public class EnhancedPassengerData
    {
        public EnhancedTravellerInformation? enhancedTravellerInformation { get; set; }
    }
    public class EnhancedTravellerInformation
    {
        public TravellerNameInfo? travellerNameInfo { get; set; }
        public OtherPaxNamesDetails? otherPaxNamesDetails { get; set; }
        public DateOfBirthInEnhancedPaxData? dateOfBirthInEnhancedPaxData { get; set; }
    }

    public class TravellerNameInfo
    {
        public string? quantity { get; set; }
        public string? type { get; set; }
        public string? infantIndicator { get; set; }
    }
    public class OtherPaxNamesDetails
    {
        public string? nameType { get; set; }
        public string? referenceName { get; set; }
        public string? displayedName { get; set; }
        public string? surname { get; set; }
        public string? givenName { get; set; }
    }
    public class DateOfBirthInEnhancedPaxData
    {
        public string? qualifier { get; set; }
        public string? date { get; set; }
    }
    public class Traveller
    {
        public string? surname { get; set; }
        public string? quantity { get; set; }
    }
    public class Passenger
    { 
        public string? firstName { get; set; }
        public string? type { get; set; }
        public string? infantIndicator { get; set; }
        public string? dob { get; set; }

    }
    
}
