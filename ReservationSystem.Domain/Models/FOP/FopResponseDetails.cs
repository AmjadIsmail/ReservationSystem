using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.FOP
{
    public class FopResponseDetails
    {
        public FOP_CreateFormOfPaymentReply? CreateFormOfPaymentReply { get;set; } 
    }
    public class FOP_CreateFormOfPaymentReply
    {
        [XmlElement(ElementName = "fopDescription")]
        public FopDescription FopDescription { get; set; }
    }

    public class FopDescription
    {
        [XmlElement(ElementName = "fopReference")]
        public FopReference FopReference { get; set; }

        [XmlElement(ElementName = "mopDescription")]
        public MopDescription MopDescription { get; set; }
    }

    public class FopReference
    {
        [XmlElement(ElementName = "reference")]
        public Reference Reference { get; set; }
    }

    public class Reference
    {
        [XmlElement(ElementName = "qualifier")]
        public string Qualifier { get; set; }

        [XmlElement(ElementName = "number")]
        public string Number { get; set; }
    }

    public class MopDescription
    {
        [XmlElement(ElementName = "fopSequenceNumber")]
        public FopSequenceNumber FopSequenceNumber { get; set; }

        [XmlElement(ElementName = "mopDetails")]
        public MopDetails MopDetails { get; set; }

        [XmlElement(ElementName = "paymentModule")]
        public object? PaymentModule { get; set; }
    }

    public class FopSequenceNumber
    {
        [XmlElement(ElementName = "sequenceDetails")]
        public SequenceDetails SequenceDetails { get; set; }
    }

    public class SequenceDetails
    {
        [XmlElement(ElementName = "number")]
        public string Number { get; set; }
    }

    public class MopDetails
    {
        [XmlElement(ElementName = "fopPNRDetails")]
        public FopPNRDetails FopPNRDetails { get; set; }

        [XmlElement(ElementName = "oldFopFreeflow")]
        public OldFopFreeflow OldFopFreeflow { get; set; }

        [XmlElement(ElementName = "pnrSupplementaryData")]
        public List<PnrSupplementaryData> PnrSupplementaryData { get; set; }
    }

    public class FopPNRDetails
    {
        [XmlElement(ElementName = "fopDetails")]
        public FopDetails FopDetails { get; set; }
    }

    public class FopDetails
    {
        [XmlElement(ElementName = "fopCode")]
        public string FopCode { get; set; }

        [XmlElement(ElementName = "fopStatus")]
        public string FopStatus { get; set; }

        [XmlElement(ElementName = "fopEdiCode")]
        public string FopEdiCode { get; set; }

        [XmlElement(ElementName = "fopReportingCode")]
        public string FopReportingCode { get; set; }

        [XmlElement(ElementName = "fopElecTicketingCode")]
        public string FopElecTicketingCode { get; set; }
    }

    public class OldFopFreeflow
    {
        [XmlElement(ElementName = "freeTextDetails")]
        public FreeTextDetails FreeTextDetails { get; set; }

        [XmlElement(ElementName = "freeText")]
        public string FreeText { get; set; }
    }

    public class FreeTextDetails
    {
        [XmlElement(ElementName = "textSubjectQualifier")]
        public string TextSubjectQualifier { get; set; }

        [XmlElement(ElementName = "source")]
        public string Source { get; set; }

        [XmlElement(ElementName = "encoding")]
        public string Encoding { get; set; }
    }

    public class PnrSupplementaryData
    {
        [XmlElement(ElementName = "dataAndSwitchMap")]
        public DataAndSwitchMap DataAndSwitchMap { get; set; }
    }

    public class DataAndSwitchMap
    {
        [XmlElement(ElementName = "criteriaSetType")]
        public string CriteriaSetType { get; set; }

        [XmlElement(ElementName = "criteriaDetails")]
        public List<CriteriaDetails> CriteriaDetails { get; set; }
    }

    public class CriteriaDetails
    {
        [XmlElement(ElementName = "attributeType")]
        public string AttributeType { get; set; }

        [XmlElement(ElementName = "attributeDescription")]
        public string AttributeDescription { get; set; }
    }
 
}
