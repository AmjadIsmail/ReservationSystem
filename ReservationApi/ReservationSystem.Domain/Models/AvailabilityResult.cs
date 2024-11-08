using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class AvailabilityResult
    {
        public Meta meta { get; set; }
        public List<FlightOffer> data { get; set; }
        public Dictionaries dictionaries { get; set; }
    }

    public class FlightPriceResult
    {
        public string? type { get; set; }
        public List<FlightOffer>? data { get; set; }
    }


































}
