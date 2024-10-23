using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.FareCheck
{
    [XmlRoot(ElementName = "Fare_CheckRulesReply", Namespace = "http://xml.amadeus.com/FARQNR_07_1_1A")]
    public class FareCheckRulesReply
    {
        public HeaderSession? Session { get; set; }

        [XmlElement(ElementName = "transactionType")]
        public TransactionType TransactionType { get; set; }

        [XmlElement(ElementName = "flightDetails")]
        public List<FlightDetailsFareCheck> FlightDetails { get; set; }

        
    }

    public class TransactionType
    {
        [XmlElement(ElementName = "messageFunctionDetails")]
        public MessageFunctionDetails MessageFunctionDetails { get; set; }
    }

    public class MessageFunctionDetails
    {
        [XmlElement(ElementName = "messageFunction")]
        public string MessageFunction { get; set; }
    }
    public class FlightDetailsFareCheck
    {
        [XmlElement(ElementName = "nbOfSegments")]
        public string NbOfSegments { get; set; }

        [XmlElement(ElementName = "qualificationFareDetails")]
        public QualificationFareDetails QualificationFareDetails { get; set; }

        [XmlElement(ElementName = "transportService")]
        public TransportService TransportService { get; set; }
        public List<FlightErrorCode>? FlightErrorCodes { get; set; }

        [XmlElement(ElementName = "fareDetailInfo")]
        public FareDetailInfo FareDetailInfo { get; set; }

        [XmlElement(ElementName = "odiGrp")]
        public OdiGrp OdiGrp { get; set; }

        [XmlElement(ElementName = "travellerGrp")]
        public TravellerGrp TravellerGrp { get; set; }

        [XmlElement(ElementName = "itemGrp")]
        public ItemGrp ItemGrp { get; set; }
    }
    public class QualificationFareDetails
    {
        [XmlElement(ElementName = "additionalFareDetails")]
        public AdditionalFareDetails AdditionalFareDetails { get; set; }
    }
    public class AdditionalFareDetails
    {
        [XmlElement(ElementName = "rateClass")]
        public string RateClass { get; set; }
    }
    public class TransportService
    {
        [XmlElement(ElementName = "companyIdentification")]
        public CompanyIdentification CompanyIdentification { get; set; }
    }
    public class FlightErrorCode
    {
        [XmlElement(ElementName = "textSubjectQualifier")]
        public string? textSubjectQualifier { get; set; }

        [XmlElement(ElementName = "informationType")]
        public string? informationType { get; set; }

        [XmlElement(ElementName = "freeText")]
        public string? freeText { get; set; }
    }
    public class CompanyIdentification
    {
        [XmlElement(ElementName = "marketingCompany")]
        public string MarketingCompany { get; set; }
    }

    public class FareDetailInfo
    {
        [XmlElement(ElementName = "nbOfUnits")]
        public NbOfUnits NbOfUnits { get; set; }

        [XmlElement(ElementName = "fareDeatilInfo")]
        public FareDetail FareDetail { get; set; }
    }
    public class NbOfUnits
    {
        [XmlElement(ElementName = "quantityDetails")]
        public List<QuantityDetails> QuantityDetails { get; set; }
    }

    public class QuantityDetails
    {
        [XmlElement(ElementName = "numberOfUnit")]
        public string NumberOfUnit { get; set; }

        [XmlElement(ElementName = "unitQualifier")]
        public string UnitQualifier { get; set; }
    }
    public class FareDetail
    {
        [XmlElement(ElementName = "fareTypeGrouping")]
        public FareTypeGrouping FareTypeGrouping { get; set; }
    }

    public class FareTypeGrouping
    {
        [XmlElement(ElementName = "pricingGroup")]
        public string PricingGroup { get; set; }
    }

    public class OdiGrp
    {
        [XmlElement(ElementName = "originDestination")]
        public OriginDestination OriginDestination { get; set; }
    }
    public class OriginDestination
    {
        [XmlElement(ElementName = "origin")]
        public string Origin { get; set; }

        [XmlElement(ElementName = "destination")]
        public string Destination { get; set; }
    }

    public class TravellerGrp
    {
        [XmlElement(ElementName = "travellerIdentRef")]
        public TravellerIdentRef TravellerIdentRef { get; set; }
    }
    public class TravellerIdentRef
    {
        [XmlElement(ElementName = "referenceDetails")]
        public ReferenceDetails ReferenceDetails { get; set; }
    }

    public class ReferenceDetails
    {
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
        [XmlElement(ElementName = "fareRulesDetails")]
        public List<string>? fareRulesDetails { get; set; }
    }
    public class ItemGrp
    {
        [XmlElement(ElementName = "itemNb")]
        public ItemNb ItemNb { get; set; }

        [XmlElement(ElementName = "unitGrp")]
        public UnitGrp UnitGrp { get; set; }
    }

    public class ItemNb
    {
        [XmlElement(ElementName = "itemNumberDetails")]
        public ItemNumberDetails ItemNumberDetails { get; set; }
    }
    public class ItemNumberDetails
    {
        [XmlElement(ElementName = "number")]
        public string Number { get; set; }
    }

    public class UnitGrp
    {
        [XmlElement(ElementName = "nbOfUnits")]
        public NbOfUnits NbOfUnits { get; set; }

        [XmlElement(ElementName = "unitFareDetails")]
        public UnitFareDetails UnitFareDetails { get; set; }
    }
    public class UnitFareDetails
    {
        [XmlElement(ElementName = "fareTypeGrouping")]
        public FareTypeGrouping FareTypeGrouping { get; set; }
    }
}
