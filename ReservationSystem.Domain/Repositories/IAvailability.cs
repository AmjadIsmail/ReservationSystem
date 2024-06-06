using ReservationSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IAvailability
    {
        public Task<string> getToken();
        public Task<List<AvailabilityModel>> GetAvailability(string token);
    }
}
