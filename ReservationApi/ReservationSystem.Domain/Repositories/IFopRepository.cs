using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.FOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IFopRepository
    {
        public Task<FopResponse> CreateFOP(FopRequest requestModel);
    }
}
