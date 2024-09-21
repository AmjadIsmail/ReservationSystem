using ReservationSystem.Domain.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IDBRepository
    {
        public Task SaveAvailabilityResult(string? requset, string? response, int totlResults);

        public Task<List<FlightMarkup>?> GetFlightMarkup();
    }
}
