using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IAvailabilityRepository
    {
        public Task<string> getToken();
        public Task<AvailabilityModel> GetAvailability(string token , AvailabilityRequest request);

        public Task ClearCache();
    }
}
