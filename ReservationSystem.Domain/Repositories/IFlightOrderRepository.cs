using ReservationSystem.Domain.Models.FlightOrder;
using ReservationSystem.Domain.Models.FlightPrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IFlightOrderRepository
    {
        public Task<FlightCreateOrderResponse> CreateFlightOrder(string token, FlightOrderCreateRequestModel requestOrder);
    }
}
