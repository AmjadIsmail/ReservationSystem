using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReservationSystem.Domain.Models.FareCheck
{
    [XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class FareCheckReplyBody
    {
        [XmlElement(ElementName = "Fare_CheckRulesReply", Namespace = "http://webservices.amadeus.com/FARQNQ_07_1_1A")]
        public FareCheckRulesReply FareCheckRulesReply { get; set; }
    }
}
