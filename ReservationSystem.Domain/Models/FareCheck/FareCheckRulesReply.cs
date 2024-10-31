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
        public HeaderSession? session { get; set; }

        [XmlElement(ElementName = "transactionType")]
        public TransactionType transactionType { get; set; }

        [XmlElement(ElementName = "flightDetails")]
        public List<FlightDetailsFareCheck> flightDetails { get; set; }


    }

    public class TransactionType
    {
        [XmlElement(ElementName = "messageFunctionDetails")]
        public MessageFunctionDetails messageFunctionDetails { get; set; }
    }

    public class MessageFunctionDetails
    {
        [XmlElement(ElementName = "messageFunction")]
        public string messageFunction { get; set; }
    }
    public class FlightDetailsFareCheck
    {
        [XmlElement(ElementName = "nbOfSegments")]
        public string nbOfSegments { get; set; }

        [XmlElement(ElementName = "qualificationFareDetails")]
        public QualificationFareDetails qualificationFareDetails { get; set; }

        [XmlElement(ElementName = "transportService")]
        public TransportService transportService { get; set; }
        public List<FlightErrorCode>? flightErrorCodes { get; set; }

        [XmlElement(ElementName = "fareDetailInfo")]
        public FareDetailInfo fareDetailInfo { get; set; }

        [XmlElement(ElementName = "odiGrp")]
        public OdiGrp odiGrp { get; set; }

        [XmlElement(ElementName = "travellerGrp")]
        public TravellerGrp travellerGrp { get; set; }

        [XmlElement(ElementName = "itemGrp")]
        public ItemGrp itemGrp { get; set; }
    }
    public class QualificationFareDetails
    {
        [XmlElement(ElementName = "additionalFareDetails")]
        public AdditionalFareDetails additionalFareDetails { get; set; }
    }
    public class AdditionalFareDetails
    {
        [XmlElement(ElementName = "rateClass")]
        public string rateClass { get; set; }
    }
    public class TransportService
    {
        [XmlElement(ElementName = "companyIdentification")]
        public CompanyIdentification companyIdentification { get; set; }
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
        public string marketingCompany { get; set; }
    }

    public class FareDetailInfo
    {
        [XmlElement(ElementName = "nbOfUnits")]
        public NbOfUnits nbOfUnits { get; set; }

        [XmlElement(ElementName = "fareDeatilInfo")]
        public FareDetail fareDetail { get; set; }
    }
    public class NbOfUnits
    {
        [XmlElement(ElementName = "quantityDetails")]
        public List<QuantityDetails> quantityDetails { get; set; }
    }

    public class QuantityDetails
    {
        [XmlElement(ElementName = "numberOfUnit")]
        public string numberOfUnit { get; set; }

        [XmlElement(ElementName = "unitQualifier")]
        public string unitQualifier { get; set; }
    }
    public class FareDetail
    {
        [XmlElement(ElementName = "fareTypeGrouping")]
        public FareTypeGrouping fareTypeGrouping { get; set; }
    }

    public class FareTypeGrouping
    {
        [XmlElement(ElementName = "pricingGroup")]
        public string pricingGroup { get; set; }
    }

    public class OdiGrp
    {
        [XmlElement(ElementName = "originDestination")]
        public OriginDestination originDestination { get; set; }
    }
    public class OriginDestination
    {
        [XmlElement(ElementName = "origin")]
        public string origin { get; set; }

        [XmlElement(ElementName = "destination")]
        public string destination { get; set; }
    }

    public class TravellerGrp
    {
        [XmlElement(ElementName = "travellerIdentRef")]
        public TravellerIdentRef travellerIdentRef { get; set; }
    }
    public class TravellerIdentRef
    {
        [XmlElement(ElementName = "referenceDetails")]
        public ReferenceDetails referenceDetails { get; set; }
    }

    public class ReferenceDetails
    {
        [XmlElement(ElementName = "type")]
        public string type { get; set; }

        [XmlElement(ElementName = "value")]
        public string value { get; set; }
        [XmlElement(ElementName = "fareRulesDetails")]
        public List<string>? fareRulesDetails { get; set; }
    }
    public class ItemGrp
    {
        [XmlElement(ElementName = "itemNb")]
        public ItemNb itemNb { get; set; }

        [XmlElement(ElementName = "unitGrp")]
        public UnitGrp unitGrp { get; set; }
    }

    public class ItemNb
    {
        [XmlElement(ElementName = "itemNumberDetails")]
        public ItemNumberDetails itemNumberDetails { get; set; }
    }
    public class ItemNumberDetails
    {
        [XmlElement(ElementName = "number")]
        public string number { get; set; }
    }

    public class UnitGrp
    {
        [XmlElement(ElementName = "nbOfUnits")]
        public NbOfUnits nbOfUnits { get; set; }

        [XmlElement(ElementName = "unitFareDetails")]
        public UnitFareDetails unitFareDetails { get; set; }
    }
    public class UnitFareDetails
    {
        [XmlElement(ElementName = "fareTypeGrouping")]
        public FareTypeGrouping fareTypeGrouping { get; set; }
    }
}