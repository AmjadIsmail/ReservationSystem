using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Root
    {
        public Meta meta { get; set; }
        public List<FlightOffer> data { get; set; }
        public Dictionaries dictionaries { get; set; }
    }
}
