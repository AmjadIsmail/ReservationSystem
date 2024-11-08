using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.PricePnr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.TicketTst
{
    public class TicketTstResponse
    {
        public HeaderSession? session { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
        public TicketTstResponseDetails? responseDetails { get; set; }
    }
}
