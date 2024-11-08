using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.AddPnrMulti
{
    public class PnrCommitRequest
    {
        public HeaderSession sessionDetails { get; set; }
        public int? optionCode1 { get; set; }
        public int? optionCode2 { get; set; }
    }
}
