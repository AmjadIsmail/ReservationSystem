using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using ReservationSystem.Domain.Service;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using ReservationSystem.Domain.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;
using ReservationSystem.Domain.Models;


namespace ReservationSystem.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IDBRepository _dBRepository;
        public EmailService(IConfiguration configuration , IDBRepository dBRepository)
        {
            _configuration = configuration;
            _dBRepository = dBRepository;
        }

        public async Task SendEmailAsync3(string toEmail, string subject, string message )
        {
           
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message // HTML Content
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailAsync2(string toEmail, string subject, string body)
        {
            var configuration = _configuration.GetSection("EmailSettings");
            var _smtpServer = _configuration["EmailSettings:SmtpServer"];
            var _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var _smtpUser = _configuration["EmailSettings:SmtpUser"];
            var _smtpPass = _configuration["EmailSettings:SmtpPass"];
            var smtpClient = new System.Net.Mail.SmtpClient(_smtpServer)
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _smtpUser,
                    _smtpPass
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var configuration = _configuration.GetSection("EmailSettings");
                var _smtpServer = _configuration["EmailSettings:SmtpServer"];
                var _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var _smtpUser = _configuration["EmailSettings:SmtpUser"];
                var _smtpPass = _configuration["EmailSettings:SmtpPass"];
                using (var client = new System.Net.Mail.SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpUser),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public string GenerateFlightConfirmationEmail(string template, Dictionary<string, string> data)
        {
            foreach (var placeholder in data)
            {
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }
            return template;
        }

        public async Task<string> GetBookingSuccessTemplate(string sessionId = "", string bookingStatus = "" , string paymentStatus = "")
        {
            try
            {
                var template = File.ReadAllText("EmailTemplates/FlightConfirmation.html");
                if (sessionId != "")
                {
                    var flightInfo = await _dBRepository.GetFlightInfo(sessionId);
                    var passengerInfo = await _dBRepository.GetPassengerInfo(sessionId);
                    FlightOffer offer = System.Text.Json.JsonSerializer.Deserialize<FlightOffer>(flightInfo?.FlightOffer);

                    var placeholders = new Dictionary<string, string>{
                  { "CustomerName", passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.FirstName + " " + passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.LastName  }, };

                    var segmentHtml = new StringBuilder(); 
                   
                    foreach (var item in offer.itineraries)
                    {
                        foreach (var segment in item.segments)
                        {
                            segmentHtml.Append($@"
            <div class='segment'>
                <table>
                    <tr><th>Flight Number:</th><td>{segment?.number}</td></tr>
                    <tr><th>Departure:</th><td>{segment?.departure?.iataName} ({segment?.departure?.iataCode}) at {segment?.departure?.at?.ToString()}</td></tr>
                    <tr><th>Arrival:</th><td>{segment?.arrival?.iataName?.ToString()} ({segment?.arrival?.iataCode?.ToString()}) at {segment?.arrival?.at?.ToString()}</td></tr>
                    <tr><th>Date:</th><td>{segment?.departure?.at?.ToString()}</td></tr>
                </table>
            </div>");
                        }                       
                    }
                    // Replace the placeholder with the actual segments
                    template = template.Replace("{{FlightSegments}}", segmentHtml.ToString());

                    StringBuilder pBuilder = new StringBuilder();
                   foreach( var p in passengerInfo)
                    {
                        pBuilder.Append($"{p.FirstName + " " + p.LastName}");                      
                        pBuilder.Append($"{p.DOB}").Append("  ");
                    }
                    placeholders.TryAdd("PassengerList", pBuilder.ToString());
                    placeholders.TryAdd("BookingStatus", bookingStatus);
                    placeholders.TryAdd("PaymentStatus", paymentStatus);
                    var emailBody = GenerateFlightConfirmationEmail(template, placeholders);                    
                    return emailBody;
                }
                else
                {
                    return "Error in sending email";
                }

              
              

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error While Sending Success booking email {ex.Message.ToString()}");
                return "Flights Booking";
            }
        }
    }
}
