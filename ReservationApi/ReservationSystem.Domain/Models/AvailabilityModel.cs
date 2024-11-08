using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models
{
    public class AvailabilityModel
    {
        public List<FlightOffer> data { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
    }
}
