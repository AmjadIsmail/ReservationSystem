using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Dictionaries
    {
        public Dictionary<string, Location> locations { get; set; }
        public Dictionary<string, string> aircraft { get; set; }
        public Dictionary<string, string> currencies { get; set; }
        public Dictionary<string, string> carriers { get; set; }
    }
}
