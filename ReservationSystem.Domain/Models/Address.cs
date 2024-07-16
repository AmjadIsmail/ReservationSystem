using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Address
    {
        public List<string>? lines { get; set; }
        public string? postalCode { get; set; }
        public string? cityName { get; set; }
        public string? countryCode { get; set; }
    }
}
