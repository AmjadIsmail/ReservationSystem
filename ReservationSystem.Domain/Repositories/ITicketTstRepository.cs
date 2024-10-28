using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.TicketTst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface ITicketTstRepository
    {
        public Task<TicketTstResponse> CreateTicketTst(TicketTstRequest requestModel);
    }
}
