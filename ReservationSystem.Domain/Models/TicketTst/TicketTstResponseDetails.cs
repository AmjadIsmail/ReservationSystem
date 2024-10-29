using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.PricePnr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.TicketTst
{
    public class TicketTstResponseDetails
    {
        public HeaderSession? session { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
        public List<TstList>? tstList { get; set; }
    }

public class TstList
    {
        [XmlElement("tstReference")]
        public TstReference TstReference { get; set; }

        [XmlElement("paxInformation")]
        public PaxInformation PaxInformation { get; set; }
    }

    public class TstReference
    {
        [XmlElement("referenceType")]
        public string ReferenceType { get; set; }

        [XmlElement("uniqueReference")]
        public int UniqueReference { get; set; }

        [XmlElement("iDDescription")]
        public IDDescription IDDescription { get; set; }
    }

    public class IDDescription
    {
        [XmlElement("iDSequenceNumber")]
        public int IDSequenceNumber { get; set; }
    }

    public class PaxInformation
    {
        [XmlElement("refDetails")]
        public RefDetails RefDetails { get; set; }
    }

    public class RefDetails
    {
        [XmlElement("refQualifier")]
        public string RefQualifier { get; set; }

        [XmlElement("refNumber")]
        public int RefNumber { get; set; }
    }

}
