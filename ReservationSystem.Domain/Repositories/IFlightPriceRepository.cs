using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.FlightPrice;

namespace ReservationSystem.Domain.Repositories
{
    public interface IFlightPriceRepository
    {
        public Task<FlightPriceModelReturn> GetFlightPrice(string token, FlightPriceModel request);
    }
}
