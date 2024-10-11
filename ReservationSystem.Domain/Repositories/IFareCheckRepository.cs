using ReservationSystem.Domain.Models.Soap.FlightPrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.FareCheck;

namespace ReservationSystem.Domain.Repositories
{
    public interface IFareCheckRepository
    {
        public Task<FareCheckReturnModel> FareCheckRequest(FareCheckModel fareCheckRequest);
    }
}
