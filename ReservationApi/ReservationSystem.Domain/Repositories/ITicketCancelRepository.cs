using ReservationSystem.Domain.Models.PnrCancel;
using ReservationSystem.Domain.Models.TicketCancel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface ITicketCancelRepository
    {
        public Task<TicketCancelResponse> CancelTicket(TicketCancelRequest requestModel);
    }
}
