using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FlightOrder
{
    public class FlightCreateOrderResponse
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? QueuingOfficeId { get; set; }
        public List<AssociatedRecord>? AssociatedRecords { get; set; }
        public List<FlightOffer>? FlightOffers { get; set; }
        public List<Traveler>? Travelers { get; set; }
        public Remarks? Remarks { get; set; }
        public TicketingAgreement? TicketingAgreement { get; set; }
        public List<AutomatedProcess>? AutomatedProcess { get; set; }
        public List<Contact>? Contacts { get; set; }

        [JsonPropertyName("dictionaries")]
        public Dictionaries? Dictionaries { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }

    }
}
