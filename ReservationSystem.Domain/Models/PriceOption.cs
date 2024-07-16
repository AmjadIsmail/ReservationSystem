using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class PriceOption
    {
        public List<string>? fareType { get; set; }
        public bool? includedCheckedBagsOnly { get; set; }
    }
}
