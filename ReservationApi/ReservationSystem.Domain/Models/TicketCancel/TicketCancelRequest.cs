using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.TicketCancel
{
    public class TicketCancelRequest
    {
        public string? ticketNumber { get; set; }
        public string? marketIataCode { get; set; }       
    }
}
