using ReservationSystem.Domain.Models.PnrCancel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Infrastructure.Repositories
{
    public interface IPnrCancelRepository
    {
        public Task<PnrCancelResponse> CancelPnr(PnrCancelRequest requestModel);
    }
}
