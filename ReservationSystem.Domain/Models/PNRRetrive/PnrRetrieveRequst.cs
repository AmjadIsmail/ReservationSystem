using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PNRRetrive
{
    public class PnrRetrieveRequst
    {        
        public string? pnrNumber { get; set; }        
        public string? retrieveType { get; set; }

        public HeaderSession? sessionDetails { get; set; }

    }
}
