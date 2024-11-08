using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.TicketTst
{
    public class TicketTstRequest
    {
        public HeaderSession? sessionDetails { get; set; }
        public int? adults { get; set; }
        public int? children { get; set; }
        public int? infants { get; set; }
    }
}
