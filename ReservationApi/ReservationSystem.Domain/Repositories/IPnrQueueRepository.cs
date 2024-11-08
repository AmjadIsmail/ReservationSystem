using ReservationSystem.Domain.Models.PnrQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IPnrQueueRepository
    {
        public Task<PnrQueueResponse> CreatePnrQueue(PnrQueueRequest requestModel);
    }
}
