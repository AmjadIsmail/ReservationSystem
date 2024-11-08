using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.PNRRetrive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IPnrRetreiveRepository
    {
        public Task<PnrRetrieveResponse> GetPnrDetails(PnrRetrieveRequst requestModel);

        public Task<PnrRetrieveResponse> RetrivePnr2(PnrRetrieveRequst requestModel);
    }
}
