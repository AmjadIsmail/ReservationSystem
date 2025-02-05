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
using ReservationSystem.Domain.Models.Enquiry;
using Azure;
using Azure.Communication.Email;
using ReservationSystem.Domain.Models.ManualPayment;
using DocumentFormat.OpenXml.Wordprocessing;


namespace ReservationSystem.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IDBRepository _dBRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailClient _emailClient;
        public EmailService(IConfiguration configuration , IDBRepository dBRepository, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _dBRepository = dBRepository;
            _environment = environment;
            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            if(connectionString == null)
            {
                connectionString = "endpoint=https://communicationservicesforjaystravels.uk.communication.azure.com/;accesskey=2cdbICCaEeMIXDjgVNMIYmliL76T9hEe9d2Y98pEpNL7lTy0OqUzJQQJ99BAACULyCpm8clbAAAAAZCSDuOI";
            }
            _emailClient = new EmailClient(connectionString);
        }

        public async Task SendEmailAsync3(string toEmail, string subject, string message)
        {
            try
            {
                string SenderName = "DoNotReply"; // Environment.GetEnvironmentVariable("EmailSettingsSenderName");
                string SenderEmail = "DoNotReply@jaystravels.co.uk"; //Environment.GetEnvironmentVariable("EmailSettingsSenderEmail");
                var bccRecipients = new List<string> ();
                List<EmailAddress> _bcc = new List<EmailAddress>();
                try
                {
                    if (_configuration["EmailSettings:BCCEmailAddress"] != null)
                    {
                        string[] cc = _configuration["EmailSettings:BCCEmailAddress"].Split(",");
                       foreach(var c in cc)
                        {
                            _bcc.Add(new EmailAddress(c));
                        }
                    }
                }
                catch
                {

                }
                var emailRecipients = new EmailRecipients(
                       to: new List<EmailAddress> { new EmailAddress(toEmail) },
                       bcc: _bcc
                   );
                var emailMessage = new EmailMessage(
                    senderAddress: SenderEmail,
                    recipients: emailRecipients,
                    content: new EmailContent(subject)
                    {
                        PlainText = subject,
                        Html = message
                    }
                );                
                EmailSendOperation emailSendOperation = _emailClient.Send(
                    WaitUntil.Completed,
                    emailMessage);               
            }
            catch (Exception ex)
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

        public async Task<string> GetEnquiryTemplate(EnquiryRequest enquiry)
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "Enquiry.html");
                var template = File.ReadAllText(filePath);
                var placeholders = new Dictionary<string, string>{
                  { "CustomerName", enquiry.FirstName + " " + enquiry.LastName  },};
                var segmentHtml = new StringBuilder();
                    segmentHtml.Append($@"
            <div class='segment'>
                 <table>
                    <tr><th>Name:</th><td>{enquiry.FirstName + " " + enquiry.LastName}</td></tr>
                    <tr><th>Email:</th><td>{enquiry.EmailAddress}</td></tr>
                    <tr><th>Phone:</th><td>{enquiry.PhoneNumber}</td></tr>
                    <tr><th>Enquriy Messege:</th><td>{enquiry.Message}</td></tr>
                </table>
            </div>");

                    // Replace the placeholder with the actual segments
                template = template.Replace("{{Enquiry}}", segmentHtml.ToString());
                var emailBody = GenerateFlightConfirmationEmail(template, placeholders);
                return emailBody;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Sending Enquiry email {ex.Message.ToString()}");
                return "Enquiry Email Error in sending Email " + ex.Message.ToString() + " " + ex.StackTrace.ToString();
            }
        }

        public async Task<string> GetBookingSuccessTemplateForAdmin(string sessionId = "", string bookingStatus = "", string paymentStatus = "")
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "AdminEmailFlightConfirmation.html");
                var template = File.ReadAllText(filePath);

                if (sessionId != "")
                {
                    var flightInfo = await _dBRepository.GetFlightInfo(sessionId);
                    var passengerInfo = await _dBRepository.GetPassengerInfo(sessionId);
                    var bookingInfo = await _dBRepository.GetBookingInfo(sessionId);
                    
                    FlightOffer offer = System.Text.Json.JsonSerializer.Deserialize<FlightOffer>(flightInfo?.FlightOffer);

                    var bookingNo = bookingInfo?.BookingRef;
                    var bookingDate = bookingInfo?.CreatedOn.Value.ToString("dd-MM-yyyy");
                    var lastTicketDate = offer.lastTicketingDate;
                    var placeholders = new Dictionary<string, string>{
                  { "BookingNo", bookingNo  }};
                    placeholders.Add("BookingDate", bookingDate);
                    placeholders.Add("TiicketDeadLine", lastTicketDate);
                   
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
                   
                    #region Search Details
                    var searcDetails = new StringBuilder();
                    searcDetails.Append($@"
            <div class='segment'>
                 <table>
                    <tr><th>From Location:</th><td>{offer?.itineraries?[0].airport_city}</td></tr>
                    <tr><th>To Location:</th><td>{offer?.itineraries?[1]?.airport_city}</td></tr>
                    <tr><th>Departure:</th><td>{offer?.itineraries[0]?.segments[0]?.departure?.at}</td></tr>
                    <tr><th>Arrival:</th><td>{offer?.itineraries[1]?.segments[0]?.departure?.at}</td></tr>
                    <tr><th>No of Passengers:</th><td> {passengerInfo.Where(e=>e.PassengerType == "ADT").Count()} Adults , {passengerInfo.Where(e => e.PassengerType == "CHD").Count()} Child , {passengerInfo.Where(e => e.PassengerType == "INF").Count()} Infants</td></tr>
                </table>
            </div>");
                      
                    // Replace the placeholder with the actual segments
                    template = template.Replace("{{SearchDetails}}", searcDetails.ToString());
                    #endregion

                    StringBuilder pBuilder = new StringBuilder();
                    foreach (var p in passengerInfo)
                    {
                        pBuilder.Append($"<div class='segment'>");
                        pBuilder.Append($"<table>");
                        pBuilder.Append($"<tr><th>Passenger Name:</th><td>{p.FirstName + " " + p.LastName}</td></tr>");
                        pBuilder.Append($"<tr><th>Passenger Type:</th><td>{p.PassengerType}</td></tr>");
                        pBuilder.Append($"<tr><th>Date of Birth:</th><td>{p.DOB}</td></tr>");
                        pBuilder.Append($"</table>");
                        pBuilder.Append($"</div>");
                    }
                    template = template.Replace("{{PassengerInformation}}", pBuilder.ToString());
                   
                    placeholders.TryAdd("BookingStatus", bookingStatus);
                    placeholders.TryAdd("PaymentStatus", paymentStatus);
                    placeholders.TryAdd("HotelInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("TransferInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("TourInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("CarInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("InsuranceInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("ParkingInformation", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("PassengerList", "<div class='segment'><span>None</span></div>");
                    placeholders.TryAdd("FareInformation", "<div class='segment'><span>None</span></div>");

                    placeholders.TryAdd("FlightPrice", offer?.price?.currency + " " + offer?.price?.total);
                    var emailBody = GenerateFlightConfirmationEmail(template, placeholders);
                    return emailBody;
                }
                else
                {
                    return "Error in sending email";
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Sending Success booking email {ex.Message.ToString()}");
                return "Flights Booking Error in sending Email " + ex.Message.ToString() + " " + ex.StackTrace.ToString();
            }
        }

        public async Task<string> GetManualPaymentTemplate(ManualPaymentCustomerDetails enquiry)
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "ManualPayment.html");
                var template = File.ReadAllText(filePath);
                var placeholders = new Dictionary<string, string>{
                  { "CustomerName", enquiry.FirstName + " " + enquiry.LastName  },                  
                };
                placeholders.Add("PaymentStatus", enquiry?.PaymentStatus.ToString() == "True" ? "Success" : "Faild");
                var segmentHtml = new StringBuilder();
                segmentHtml.Append($@"
            <div class='segment'>
                 <table>
                    <tr><th>Booking Ref:</th><td>{enquiry.BookingRef}</td></tr>
                    <tr><th>Amount:</th><td>{enquiry.Amount}</td></tr>
                    <tr><th>Name:</th><td>{enquiry.FirstName + " " + enquiry.LastName}</td></tr>
                    <tr><th>Email:</th><td>{enquiry.Email}</td></tr>
                    <tr><th>Phone:</th><td>{enquiry.Phone}</td></tr>
                    <tr><th>Address:</th><td>{enquiry.Address}</td></tr>
                    <tr><th>City:</th><td>{enquiry.City}</td></tr>
                    <tr><th>Country:</th><td>{enquiry.Country}</td></tr>
                    <tr><th>Postal:</th><td>{enquiry.Postal}</td></tr>
                </table>
            </div>");

                // Replace the placeholder with the actual segments
                template = template.Replace("{{PaymentDetails}}", segmentHtml.ToString());
                var emailBody = GenerateFlightConfirmationEmail(template, placeholders);
                return emailBody;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Sending Enquiry email {ex.Message.ToString()}");
                return "Enquiry Email Error in sending Email " + ex.Message.ToString() + " " + ex.StackTrace.ToString();
            }
        }

        public async Task<string> GetPassengerSelectedFlightTemplate(string sessionId = "")
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "PassengerSelectedFlight.html");
                var template = File.ReadAllText(filePath);

                if (sessionId != "")
                {
                    var flightInfo = await _dBRepository.GetFlightInfo(sessionId);
                    var passengerInfo = await _dBRepository.GetPassengerInfo(sessionId);
                    var bookingInfo = await _dBRepository.GetBookingInfo(sessionId);
                    FlightOffer offer = System.Text.Json.JsonSerializer.Deserialize<FlightOffer>(flightInfo?.FlightOffer);

                    var placeholders = new Dictionary<string, string>{
                  { "CustomerName", passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.FirstName + " " + passengerInfo.Where(e=>e.IsLead == true).FirstOrDefault()?.LastName  }, };

                    var segmentHtml = new StringBuilder();
                    string FlightPrice = offer.price?.total;
                    string AdultMarkup = offer.price?.adulMarkup;
                    string ChildMarkup = offer.price?.childMarkup;
                    string InfantMarkup = offer.price.infantMarkup;
                    string FlightMarkupID = offer?.MarkupId.ToString();
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
                    template = template.Replace("{{FlightPrice}}", FlightPrice.ToString());
                    template = template.Replace("{{AdultMarkup}}", AdultMarkup.ToString());
                    template = template.Replace("{{ChildMarkup}}", ChildMarkup.ToString());
                    template = template.Replace("{{InfantMarkup}}", InfantMarkup.ToString());
                    template = template.Replace("{{FlightMarkupID}}", FlightMarkupID.ToString());

                    StringBuilder pBuilder = new StringBuilder();
                    foreach (var p in passengerInfo)
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
                    placeholders.TryAdd("FlightPrice", offer?.price?.currency + " " + offer?.price?.total);
                    var emailBody = GenerateFlightConfirmationEmail(template, placeholders);
                    return emailBody;
                }
                else
                {
                    return "Error in sending email";
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Sending Success booking email {ex.Message.ToString()}");
                return "Flights Booking Error in sending Email " + ex.Message.ToString() + " " + ex.StackTrace.ToString();
            }
        }

    }
}
