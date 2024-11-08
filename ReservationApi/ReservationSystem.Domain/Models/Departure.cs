using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Departure
    {
        public string? iataCode { get; set; }
        public string? iataName { get; set; }
        public string? terminal { get; set; }
        public DateTime? at { get; set; }
    }
}
