using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PnrCancel
{
    public class PnrCancelRequest
    {
        public string? pnr { get; set; }
        public string? optionCode { get; set; }
        public string? entryType { get; set; }
    }
}
