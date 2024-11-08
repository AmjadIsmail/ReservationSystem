using ReservationSystem.Domain.Models.FOP;
using ReservationSystem.Domain.Models.PricePnr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IPricePnrRepository
    {
        public Task<PricePnrResponse> CreatePricePNRWithBookingClass(PricePnrRequest requestModel);
    }
}
