using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FareCheck
{
    public class FareCheckModel
    {
        public List<string>? typeQualifier { get; set; }
        public List<int>? itemNumber { get; set; }
        public string? FcType { get; set; }
        public HeaderSession sessionDetails { get; set; }
    }
}