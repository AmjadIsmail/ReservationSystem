using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class ContactInfo
    {
        public string? emailAddress { get; set; }
        public List<Phone>? Phones { get; set; }
    }
}
