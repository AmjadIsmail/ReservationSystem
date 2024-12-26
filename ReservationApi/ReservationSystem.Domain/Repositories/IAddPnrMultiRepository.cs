using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IAddPnrMultiRepository
    {
        public Task<AddPnrMultiResponse> AddPnrMulti(AddPnrMultiRequset requestModel);

        public Task<PnrCommitResponse?> CommitPNR(PnrCommitRequest requestModel);
        public Task Security_Signout(HeaderSession header);

        public Task<bool> UpdatePaymentStatusInBookingInfo(UpdatePaymentStatus requestModel);
    }
}
