using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FlightOrder
{
    public class FlightOrderCreateRequestModel
    {
        public string? type { get; set; } = "flight-order";
        public List<FlightOffer>? flightOffers { get; set; }
        public List<Traveler>? travelers { get; set; }
        public Remarks? remarks { get; set; }
        public TicketingAgreement? ticketingAgreement { get; set; }
        public List<Contact>? contacts { get; set; }

    }
}
