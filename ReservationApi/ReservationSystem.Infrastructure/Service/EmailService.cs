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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;


namespace ReservationSystem.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IDBRepository _dBRepository;
        private readonly IWebHostEnvironment _environment;
        public EmailService(IConfiguration configuration , IDBRepository dBRepository, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _dBRepository = dBRepository;
            _environment = environment;
        }

        public async Task SendEmailAsync3(string toEmail, string subject, string message )
        {
            try
            {
                string SmtpServer = Environment.GetEnvironmentVariable("EmailSettingsServer");
                string SenderName = Environment.GetEnvironmentVariable("EmailSettingsSenderName");
                string SenderEmail = Environment.GetEnvironmentVariable("EmailSettingsSenderEmail");
                string SmtpUser = Environment.GetEnvironmentVariable("EmailSettingsSmtpUser");
                string SmtpPass = Environment.GetEnvironmentVariable("EmailSettingsSmtpPass");
                string SmtpPort = Environment.GetEnvironmentVariable("EmailSettingsSmtpPort");

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(SenderName, SenderEmail));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Subject = subject;
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message // HTML Content
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    //await client.ConnectAsync(SmtpServer, int.Parse(SmtpPort), MailKit.Security.SecureSocketOptions.StartTls);
                    //await client.AuthenticateAsync(SmtpUser, SmtpPass);
                    //await client.SendAsync(emailMessage);
                    //await client.DisconnectAsync(true);
                    client.Connect(SmtpServer, int.Parse(SmtpPort), MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(SmtpUser, SmtpPass);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error while sending email {ex.StackTrace.ToString()}");
            }
           
        }

        public async Task SendEmailAsync2(string toEmail, string subject, string body)
        {
            string SmtpServer = Environment.GetEnvironmentVariable("EmailSettingsServer");
            string SenderName = Environment.GetEnvironmentVariable("EmailSettingsSenderName");
            string SenderEmail = Environment.GetEnvironmentVariable("EmailSettingsSenderEmail");
            string SmtpUser = Environment.GetEnvironmentVariable("EmailSettingsSmtpUser");
            string SmtpPass = Environment.GetEnvironmentVariable("EmailSettingsSmtpPass");
            string SmtpPort = Environment.GetEnvironmentVariable("EmailSettingsSmtpPort");            
            try {
                var smtpClient = new System.Net.Mail.SmtpClient(SmtpServer)
                {
                    Port = 587,
                    Credentials = new NetworkCredential(
                        SmtpUser,
                        SmtpPass
                    ),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);
                smtpClient.Send(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
             }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

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
                var filePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "FlightConfirmation.html");
                var template = File.ReadAllText(filePath);
            
                if (sessionId != "")
                {
                    var flightInfo = await _dBRepository.GetFlightInfo(sessionId);
                    var passengerInfo = await _dBRepository.GetPassengerInfo(sessionId);
                    var bookingInfo = await _dBRepository.GetBookingInfo(sessionId);
                    FlightOffer offer = System.Text.Json.JsonSerializer.Deserialize<FlightOffer>(flightInfo?.FlightOffer);

                    var placeholders = new Dictionary<string, string>{
                  { "CustomerName", passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.FirstName + " " + passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.LastName  }, };


                    StringBuilder bookingref = new StringBuilder();
                    bookingref.Append($"<div class='segment'>");
                    bookingref.Append($"<table>");
                    bookingref.Append($"<tr><th>Booking Reference:</th><td>{bookingInfo.BookingRef}</td></tr>");
                    bookingref.Append($"</table>");
                    bookingref.Append($"</div>");
                    
                    template = template.Replace("{{BookingRef}}", bookingref.ToString());
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
                        pBuilder.Append($"<div class='segment'>");
                        pBuilder.Append($"<table>");
                        pBuilder.Append($"<tr><th>Passenger Name:</th><td>{p.FirstName + " " + p.LastName}</td></tr>");
                        pBuilder.Append($"<tr><th>Passenger Type:</th><td>{p.PassengerType}</td></tr>");
                        pBuilder.Append($"<tr><th>Date of Birth:</th><td>{p.DOB}</td></tr>");
                        pBuilder.Append($"</table>");
                        pBuilder.Append($"</div>");
                    }
                    template = template.Replace("{{PassengerList}}", pBuilder.ToString());
                    //placeholders.TryAdd("PassengerList", pBuilder.ToString());
                    placeholders.TryAdd("BookingStatus", bookingStatus);
                    placeholders.TryAdd("PaymentStatus", paymentStatus);
                    placeholders.TryAdd("FlightPrice",  offer?.price?.currency +" " + offer?.price?.total);
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
                return "Flights Booking Error in sending Email " + ex.Message.ToString() + " " + ex.StackTrace.ToString();
            }
        }
    }
}
