using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.DocIssueTicket
{
    public class DocIssueTicketRequest
    {
        public string? indicator { get; set; }
        public HeaderSession sessionDetails { get; set; }
    }
}
