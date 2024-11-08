using ReservationSystem.Domain.Models.DocIssueTicket;
using ReservationSystem.Domain.Models.PnrQueue;
using ReservationSystem.Domain.Models.PNRRetrive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IDocIssueTicketRepository
    {
        public Task<DocIssueTicketResponse> DocIssueTicket(DocIssueTicketRequest requestModel);

      
    }
}
