using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class HeaderSession
    {
        public string? TransactionStatusCode { get; set; }
        public string? SessionId { get; set; }
        public int? SequenceNumber { get; set; }
        public string? SecurityToken { get; set; }
    }
}
