using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Service
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string body);

        public Task SendEmailAsync2(string toEmail, string subject, string body);

        public Task SendEmailAsync3(string toEmail, string subject, string message);

        public Task<string> GetBookingSuccessTemplate(string sessionId = "", string bookingStatus = "", string paymentStatus = "");
    }
}
