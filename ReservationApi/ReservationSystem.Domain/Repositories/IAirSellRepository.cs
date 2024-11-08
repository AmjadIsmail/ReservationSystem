using ReservationSystem.Domain.Models.Soap.FlightPrice;
using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;

namespace ReservationSystem.Domain.Repositories
{
    public interface IAirSellRepository
    {
        public Task<AirSellFromRecResponseModel> GetAirSellRecommendation(AirSellFromRecommendationRequest requestModel);
    }
}
