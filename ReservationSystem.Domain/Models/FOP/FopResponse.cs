using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FOP
{
    public class FopResponse
    {
        public HeaderSession? session { get; set; }
        public FopResponseDetails? responseDetails { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
     
       
    }
}
