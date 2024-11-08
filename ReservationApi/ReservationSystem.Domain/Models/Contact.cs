using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class Contact
    {
        public AddresseeName? addresseeName { get; set; }
        public string? companyName { get; set; }
        public string? purpose { get; set; }
        public List<Phone>? phones { get; set; }
        public string? emailAddress { get; set; }
        public Address? address { get; set; }
    }
}
