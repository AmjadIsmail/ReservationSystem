using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Itinerary
    {
        public string ? flightProposal_ref { get; set; }
        public string? duration { get; set; }
        public List<Segment>? segments { get; set; }
        public string? segment_type { get; set; }
        public string? airport_city { get; set; }
    }
}
