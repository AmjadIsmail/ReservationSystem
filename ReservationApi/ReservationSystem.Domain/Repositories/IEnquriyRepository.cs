using ReservationSystem.Domain.Models.Enquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IEnquriyRepository
    {
        public Task<bool> SendEnquiry(EnquiryRequest requestModel);
    }
}
