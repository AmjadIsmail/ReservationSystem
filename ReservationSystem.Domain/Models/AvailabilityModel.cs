using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class AvailabilityModel
    {
        public string type { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public string instantTicketingRequired { get; set; }
        public string nonHomogeneous { get; set; }
        public string oneWay { get; set; }
        public string lastTicketingDate { get; set; }
        public string lastTicketingDateTime { get; set; }
        public string numberOfBookableSeats { get; set; }
    }
}
