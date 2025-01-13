using ReservationSystem.Domain.Models.ManualPayment;
using ReservationSystem.Domain.Models.Payment;
using ReservationSystem.Domain.Models.PnrCancel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Repositories
{
    public interface IPaymentRepository
    {
        public Task<PaymentResponse> GeneratePaymentRequest(PaymentRequest request);

        public Task<PaymentResponse> GenerateManualPaymentRequest(PaymentRequest request);

        public Task<bool> VerifyShaOutSignature(Dictionary<string, string> form);
        public Task<bool> UpdateManualPaymentDetails(ManualPaymentCustomerDetails request);
    }
}
