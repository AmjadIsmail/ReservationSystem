using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Domain.Models.Email;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService; 
        }
   
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            if (emailRequest == null || string.IsNullOrEmpty(emailRequest.ToEmail))
                return BadRequest("Invalid email request.");
            var emailBody = await _emailService.GetBookingSuccessTemplate();

            await _emailService.SendEmailAsync3(emailRequest.ToEmail, emailRequest.Subject,emailBody);

            return Ok("Email sent successfully.");
        }   

    }
}
