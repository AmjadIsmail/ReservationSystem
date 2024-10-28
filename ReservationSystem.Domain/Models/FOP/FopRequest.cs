using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FOP
{
    public class FopRequest
    {
        public HeaderSession? sessionDetails { get; set; }
        public string? transactionDetailsCode { get; set; }
       public string? fopCode { get; set; }
    }
}
