using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.FareCheck
{

    public class FareCheckReturnModel
    {

        public FareCheckRulesReply data { get; set; }
        public AmadeusResponseError? amadeusError { get; set; }
    }
}