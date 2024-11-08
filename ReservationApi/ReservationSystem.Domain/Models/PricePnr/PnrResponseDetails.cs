using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.PricePnr
{
    public class PnrResponseDetails
    {
        [XmlElement(ElementName = "Fare_PricePNRWithBookingClassReply", Namespace = "http://xml.amadeus.com/TPCBRR_23_2_1A")]
        public FarePricePNRWithBookingClassReply? FarePricePNRWithBookingClassReply { get; set; }
    }
    public class FarePricePNRWithBookingClassReply
    {
        [XmlElement(ElementName = "fareList")]
        public List<FareList>? FareList { get; set; }
    }
    public class PricingInformation
    {
        [XmlElement(ElementName = "tstInformation")]
        public TstInformation? TstInformation { get; set; }

        [XmlElement(ElementName = "fcmi")]
        public int? Fcmi { get; set; }
    }

    public class TstInformation
    {
        [XmlElement(ElementName = "tstIndicator")]
        public string? TstIndicator { get; set; }
    }
    public class FareReference
    {
        [XmlElement(ElementName = "referenceType")]
        public string? ReferenceType { get; set; }
        [XmlElement(ElementName = "uniqueReference")]
        public string? UniqueReference { get; set; }
    }
    public class LastTicketDate
    {
        [XmlElement(ElementName = "businessSemantic")]
        public string? BusinessSemantic { get; set; }
        [XmlElement(ElementName = "dateTime")]
        public DateTimeT? DateTimeT { get; set; }
        public DateTime? FullDate { get; set; }
    }
  
    public class DateTimeT
    {
        public string? year { get; set; }
        public string? month { get; set; }
        public string? day { get; set; }
        public string? hour { get; set; }
        public string? minutes { get; set; }
    }
    public class ValidatingCarrier
    {
        [XmlElement("carrierInformation")]
        public CarrierInformation? CarrierInformation { get; set; }
    }

    public class CarrierInformation
    {
        [XmlElement("carrierCode")]
        public string? CarrierCode { get; set; }
    }
    public class PaxSegReference
    {
        [XmlElement("refDetails")]
        public RefDetails? RefDetails { get; set; }
    }

    public class RefDetails
    {
        [XmlElement("refQualifier")]
        public string? RefQualifier { get; set; }

        [XmlElement("refNumber")]
        public string? RefNumber { get; set; }
    }
    public class FareDataInformation
    {
        [XmlElement("fareDataMainInformation")]
        public FareDataMainInformation? FareDataMainInformation { get; set; }

        [XmlElement("fareDataSupInformation")]
        public List<FareDataSupInformation>? FareDataSupInformation { get; set; }
    }

  
    public class FareDataSupInformation
    {
        [XmlElement("fareDataQualifier")]
        public string? FareDataQualifier { get; set; }

        [XmlElement("fareAmount")]
        public decimal FareAmount { get; set; }

        [XmlElement("fareCurrency")]
        public string? FareCurrency { get; set; }
    }
    public class OfferReferences
    {
        [XmlElement("offerIdentifier")]
        public OfferIdentifier? OfferIdentifier { get; set; }
    }

    public class OfferIdentifier
    {
        [XmlElement("uniqueOfferReference")]
        public string? UniqueOfferReference { get; set; }
    }
    public class TaxInformation
    {
        [XmlElement("taxDetails")]
        public TaxDetails? TaxDetails { get; set; }

        [XmlElement("amountDetails")]
        public AmountDetails AmountDetails { get; set; }
    }

    public class TaxDetails
    {
        [XmlElement("taxQualifier")]
        public string? TaxQualifier { get; set; }

        [XmlElement("taxIdentification")]
        public TaxIdentification TaxIdentification { get; set; }

        [XmlElement("taxType")]
        public TaxType? TaxType { get; set; }

        [XmlElement("taxNature")]
        public string? TaxNature { get; set; }
    }

    public class TaxIdentification
    {
        [XmlElement("taxIdentifier")]
        public string? TaxIdentifier { get; set; }
    }

    public class TaxType
    {
        [XmlElement("isoCountry")]
        public string? IsoCountry { get; set; }
    }

    public class AmountDetails
    {
        [XmlElement("fareDataMainInformation")]
        public FareDataMainInformation? FareDataMainInformation { get; set; }
    }

    public class FareDataMainInformation
    {
        [XmlElement("fareDataQualifier")]
        public string? FareDataQualifier { get; set; }
        [XmlElement("fareAmount")]
        public string? FareAmount { get; set; } 
        [XmlElement("fareCurrency")]
        public string? FareCurrency { get; set; } 

    }
    public class OriginDestination
    {
        [XmlElement("cityCode")]
        public string? CityCode { get; set; }
    }
    [XmlRoot("segmentInformation")]
    public class SegmentInformation
    {
        [XmlElement("connexInformation")]
        public ConnexInformation? ConnexInformation { get; set; }

        [XmlElement("segDetails")]
        public SegDetails? SegDetails { get; set; }

        [XmlElement("fareQualifier")]
        public FareQualifier? FareQualifier { get; set; }

        [XmlElement("cabinGroup")]
        public CabinGroup? CabinGroup { get; set; }

        [XmlElement("bagAllowanceInformation")]
        public BagAllowanceInformation? BagAllowanceInformation { get; set; }

        [XmlElement("segmentReference")]
        public SegmentReference? SegmentReference { get; set; }

        [XmlElement("sequenceInformation")]
        public SequenceInformation? SequenceInformation { get; set; }

        [XmlElement("flightProductInformationType")]
        public FlightProductInformationType? FlightProductInformationType { get; set; }
    }

    public class ConnexInformation
    {
        [XmlElement("connecDetails")]
        public ConnecDetails? ConnecDetails { get; set; }
    }

    public class ConnecDetails
    {
        [XmlElement("connexType")]
        public string? ConnexType { get; set; }
    }

    public class SegDetails
    {
        [XmlElement("segmentDetail")]
        public SegmentDetail? SegmentDetail { get; set; }
    }

    public class SegmentDetail
    {
        [XmlElement("identification")]
        public string? Identification { get; set; }

        [XmlElement("classOfService")]
        public string? ClassOfService { get; set; }
    }

    public class FareQualifier
    {
        [XmlElement("fareBasisDetails")]
        public FareBasisDetails? FareBasisDetails { get; set; }
    }

    public class FareBasisDetails
    {
        [XmlElement("primaryCode")]
        public string? PrimaryCode { get; set; }

        [XmlElement("fareBasisCode")]
        public string? FareBasisCode { get; set; }

        [XmlElement("ticketDesignator")]
        public string? TicketDesignator { get; set; }

        [XmlElement("discTktDesignator")]
        public string? DiscTktDesignator { get; set; }
    }

    public class CabinGroup
    {
        [XmlElement("cabinSegment")]
        public CabinSegment? CabinSegment { get; set; }
    }

    public class CabinSegment
    {
        [XmlElement("bookingClassDetails")]
        public BookingClassDetails? BookingClassDetails { get; set; }
    }

    public class BookingClassDetails
    {
        [XmlElement("designator")]
        public string? Designator { get; set; }

        [XmlElement("option")]
        public string? Option { get; set; }
    }

    public class BagAllowanceInformation
    {
        [XmlElement("bagAllowanceDetails")]
        public BagAllowanceDetails? BagAllowanceDetails { get; set; }
    }

    public class BagAllowanceDetails
    {
        [XmlElement("baggageQuantity")]
        public int? BaggageQuantity { get; set; }

        [XmlElement("baggageType")]
        public string? BaggageType { get; set; }
    }

    public class SegmentReference
    {
        [XmlElement("refDetails")]
        public RefDetails? RefDetails { get; set; }    }

  

    public class SequenceInformation
    {
        [XmlElement("sequenceSection")]
        public SequenceSection? SequenceSection { get; set; }
    }

    public class SequenceSection
    {
        [XmlElement("sequenceNumber")]
        public int? SequenceNumber { get; set; }
    }

    public class FlightProductInformationType
    {
        [XmlElement("cabinProduct")]
        public CabinProduct? CabinProduct { get; set; }
    }

    public class CabinProduct
    {
        [XmlElement("rbd")]
        public string? Rbd { get; set; }

        [XmlElement("cabin")]
        public string? Cabin { get; set; }

        [XmlElement("avlStatus")]
        public string? AvlStatus { get; set; }
    }
    [XmlRoot("otherPricingInfo")]
    public class OtherPricingInfo
    {
        [XmlElement("attributeDetails")]
        public AttributeDetails? AttributeDetails { get; set; }
    }

    public class AttributeDetails
    {
        [XmlElement("attributeType")]
        public string? AttributeType { get; set; }

        [XmlElement("attributeDescription")]
        public string? AttributeDescription { get; set; }
    }
    [XmlRoot("warningInformation")]
    public class WarningInformation
    {
        [XmlElement("warningCode")]
        public WarningCode? WarningCode { get; set; }

        [XmlElement("warningText")]
        public WarningText? WarningText { get; set; }
    }

    public class WarningCode
    {
        [XmlElement("applicationErrorDetail")]
        public ApplicationErrorDetail? ApplicationErrorDetail { get; set; }
    }

    public class ApplicationErrorDetail
    {
        [XmlElement("applicationErrorCode")]
        public string? ApplicationErrorCode { get; set; }

        [XmlElement("codeListQualifier")]
        public string? CodeListQualifier { get; set; }

        [XmlElement("codeListResponsibleAgency")]
        public string? CodeListResponsibleAgency { get; set; }
    }

    public class WarningText
    {
        [XmlElement("errorFreeText")]
        public string? ErrorFreeText { get; set; }
    }
    [XmlRoot("fareComponentDetailsGroup")]
    public class FareComponentDetailsGroup
    {
        [XmlElement("fareComponentID")]
        public FareComponentID? FareComponentID { get; set; }

        [XmlElement("marketFareComponent")]
        public MarketFareComponent? MarketFareComponent { get; set; }

        [XmlElement("monetaryInformation")]
        public MonetaryInformation? MonetaryInformation { get; set; }

        [XmlElement("componentClassInfo")]
        public ComponentClassInfo? ComponentClassInfo { get; set; }

        [XmlElement("fareQualifiersDetail")]
        public FareQualifiersDetail? FareQualifiersDetail { get; set; }

        [XmlElement("fareFamilyDetails")]
        public FareFamilyDetails? FareFamilyDetails { get; set; }

        [XmlElement("fareFamilyOwner")]
        public FareFamilyOwner? FareFamilyOwner { get; set; }

        [XmlElement("couponDetailsGroup")]
        public CouponDetailsGroup? CouponDetailsGroup { get; set; }
    }

    public class FareComponentID
    {
        [XmlElement("itemNumberDetails")]
        public ItemNumberDetails? ItemNumberDetails { get; set; }
    }

    public class ItemNumberDetails
    {
        [XmlElement("number")]
        public string? Number { get; set; }

        [XmlElement("type")]
        public string? Type { get; set; }
    }

    public class MarketFareComponent
    {
        [XmlElement("boardPointDetails")]
        public BoardPointDetails? BoardPointDetails { get; set; }

        [XmlElement("offpointDetails")]
        public OffpointDetails? OffpointDetails { get; set; }
    }

    public class BoardPointDetails
    {
        [XmlElement("trueLocationId")]
        public string? TrueLocationId { get; set; }
    }

    public class OffpointDetails
    {
        [XmlElement("trueLocationId")]
        public string? TrueLocationId { get; set; }
    }

    public class MonetaryInformation
    {
        [XmlElement("monetaryDetails")]
        public MonetaryDetails? MonetaryDetails { get; set; }
    }

    public class MonetaryDetails
    {
        [XmlElement("typeQualifier")]
        public string? TypeQualifier { get; set; }

        [XmlElement("amount")]
        public decimal? Amount { get; set; }

        [XmlElement("currency")]
        public string? Currency { get; set; }
    }

    public class ComponentClassInfo
    {
        [XmlElement("fareBasisDetails")]
        public SegmentFareBasisDetails? FareBasisDetails { get; set; }
    }

    public class SegmentFareBasisDetails
    {
        [XmlElement("rateTariffClass")]
        public string? RateTariffClass { get; set; }

        [XmlElement("otherRateTariffClass")]
        public string? OtherRateTariffClass { get; set; }
    }

    public class FareQualifiersDetail
    {
        [XmlElement("discountDetails")]
        public DiscountDetails? DiscountDetails { get; set; }
    }

    public class DiscountDetails
    {
        [XmlElement("fareQualifier")]
        public string? FareQualifier { get; set; }
    }

    public class FareFamilyDetails
    {
        [XmlElement("fareFamilyname")]
        public string? FareFamilyName { get; set; }

        [XmlElement("hierarchy")]
        public int? Hierarchy { get; set; }
    }

    public class FareFamilyOwner
    {
        [XmlElement("companyIdentification")]
        public CompanyIdentification? CompanyIdentification { get; set; }
    }

    public class CompanyIdentification
    {
        [XmlElement("otherCompany")]
        public string? OtherCompany { get; set; }
    }

    public class CouponDetailsGroup
    {
        [XmlElement("productId")]
        public ProductId? ProductId { get; set; }
    }

    public class ProductId
    {
        [XmlElement("referenceDetails")]
        public ReferenceDetails? ReferenceDetails { get; set; }
    }

    public class ReferenceDetails
    {
        [XmlElement("type")]
        public string? Type { get; set; }

        [XmlElement("value")]
        public string? Value { get; set; }
    }

    public class FareList
    {
        [XmlElement(ElementName = "pricingInformation")]
        public PricingInformation? PricingInformation { get; set; }

        [XmlElement(ElementName = "fareReference")]
        public FareReference? FareReference { get; set; }

        [XmlElement(ElementName = "lastTktDate")]
        public LastTicketDate? LastTktDate { get; set; }

        [XmlElement(ElementName = "validatingCarrier")]
        public ValidatingCarrier? ValidatingCarrier { get; set; }

        [XmlElement(ElementName = "paxSegReference")]
        public PaxSegReference? PaxSegReference { get; set; }

        [XmlElement(ElementName = "fareDataInformation")]
        public FareDataInformation? FareDataInformation { get; set; }

        [XmlElement(ElementName = "offerReferences")]
        public OfferReferences? OfferReferences { get; set; }

        [XmlElement(ElementName = "taxInformation")]
        public List<TaxInformation>? TaxInformation { get; set; }

        [XmlElement(ElementName = "originDestination")]
        public OriginDestination? OriginDestination { get; set; }

        [XmlElement(ElementName = "segmentInformation")]
        public List<SegmentInformation>? SegmentInformation { get; set; }

        [XmlElement(ElementName = "otherPricingInfo")]
        public OtherPricingInfo? OtherPricingInfo { get; set; }

        [XmlElement(ElementName = "warningInformation")]
        public List<WarningInformation>? WarningInformation { get; set; }

        [XmlElement(ElementName = "fareComponentDetailsGroup")]
        public List<FareComponentDetailsGroup>? FareComponentDetailsGroup { get; set; }
    }

}
