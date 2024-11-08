using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReservationSystem.Domain.Models.FlightOrder
{
    public class FlightOrderRequest
    {
        [JsonPropertyName("data")]
        public data data { get; set; }
    }
    public class data
    {
        [JsonPropertyName("type")]
        public string type { get; set; }
        public List<FlightOfferForOrder>? flightOffers { get; set; }
        public List<Traveler>? travelers { get; set; }
        public Remarks? remarks { get; set; }
        public TicketingAgreement? ticketingAgreement { get; set; }
        public List<Contact>? contacts { get; set; }

    }
}
