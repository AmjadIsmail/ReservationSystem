using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Phone
    {
        public string? deviceType { get; set; }
        public string? countryCallingCode { get; set; }
        public string? number { get; set; }
    }
}
