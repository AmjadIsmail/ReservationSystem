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
<<<<<<< HEAD
=======
        public HeaderSession sessionDetails { get; set; }
>>>>>>> 327b9c02008c178a3d19c315f50d1a405d46bcb1
    }
}
