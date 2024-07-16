using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ReservationSystem.Domain.Models
{
    public class Traveler
    {
        public string? id { get; set; }
        public string? dateOfBirth { get; set; }
        public Name? name { get; set; }
        public string? gender { get; set; }
        public ContactInfo? contact { get; set; }
        public List<Document>? documents { get; set; }
    }
}
