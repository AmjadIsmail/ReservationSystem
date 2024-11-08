using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface ITravelBoardSearchRepository
    {

        

        public  Task<AvailabilityModel> GetAvailability(AvailabilityRequest requestModel);
        public Task ClearCache();
    }
}
