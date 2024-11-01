using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.FOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.PricePnr
{
    public class PricePnrResponse
    {
        public HeaderSession? session { get; set; }
        public string? warningDetails { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
        public PnrResponseDetails? responseDetails { get; set; }
       
    }
}
