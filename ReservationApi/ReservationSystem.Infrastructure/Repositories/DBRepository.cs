using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReservationApi.ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.DBContext;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.DBLogs;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class DBRepository : IDBRepository
    {
        private DB_Context _context;
        private readonly ILogger<DBRepository> _logger;
        public DBRepository(IConfiguration configuration, DB_Context context, ILogger<DBRepository> logger)
        {
           _context = context;
            _logger = logger;
        }

        public async Task SaveAvailabilityResult(string? requset , string? response , int totlResults)
        {
            try
            {
                var Res = new SearchAvailabilityResults();
                Res.created_on = DateTime.Now.ToUniversalTime();
                Res.request = requset;
                Res.response = response;
                Res.total_results = totlResults;
                Res.user_id = 0;
                await _context.AvailabilityResults.AddAsync(Res);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while saving Search Result {ex.Message.ToString()}");
            }
          
           
        }

        private string GetJsonRequest(string Envelope)
        {
            try{
                XmlDocument xmlReq = new XmlDocument();
                xmlReq.LoadXml(Envelope);
                return JsonConvert.SerializeXmlNode(xmlReq, Newtonsoft.Json.Formatting.Indented);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while conver request to json {ex.Message.ToString()}");
                return Envelope;
            }
        }
        public async Task SaveReservationFlow(SaveReservationLog requset)
        {
            try
            {
                var Res = new ReservationFlow();
                Res.CreatedOn = DateTime.Now.ToUniversalTime();
                Res.Request = GetJsonRequest(requset.Request);
                Res.Response = requset.Response;
                Res.RequestName = requset.RequestName;
                Res.AmadeusSessionId = requset.AmadeusSessionId;
                Res.UserId = 0;                
                await _context.ReservationFlow.AddAsync(Res);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while saving Search Result {ex.Message.ToString()}");
            }


        }

        public async Task SavePassengerInfo(AddPnrMultiRequset request)
        {
            try
            {
                int flightId = 1;
                #region  Saving Flight Info
                try {
                    if(request.selectedFlightOffer != null)
                    {
                        FlightOffer offer = System.Text.Json.JsonSerializer.Deserialize<FlightOffer>(request?.selectedFlightOffer);
                        var flightinfo = new FlightInfo();
                        flightinfo.AmadeusSessionId = request?.sessionDetails?.SessionId;                        
                        flightinfo.ArrivalTime = offer?.itineraries?.Count > 1 ?  offer?.itineraries[1]?.segments?.FirstOrDefault()?.departure?.at : offer?.itineraries?[0]?.segments?.TakeLast(1)?.FirstOrDefault()?.arrival?.at;
                        flightinfo.Destination = offer?.itineraries?.Count > 1 ? offer?.itineraries[1]?.segments?.FirstOrDefault()?.departure?.iataName : offer?.itineraries?[0]?.segments?.TakeLast(1)?.FirstOrDefault()?.arrival?.iataName;
                        flightinfo.CabinClass = offer?.itineraries?[0]?.segments?[0]?.cabinClass;
                        flightinfo.CreatedOn = DateTime.UtcNow;
                        flightinfo.Departure = offer?.itineraries?[0]?.segments?[0]?.departure?.iataName;
                        flightinfo.DepartureTime = offer?.itineraries?[0]?.segments?[0]?.departure?.at;
                        flightinfo.FlightNumber = offer?.itineraries?[0]?.segments?[0]?.number;
                        flightinfo.FlightOffer = request?.selectedFlightOffer;
                        await _context.FlightsInfo.AddAsync(flightinfo);
                        await _context.SaveChangesAsync();
                        flightId = flightinfo.FlightId;

                    }
                
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Error while saving Flight Details {ex.Message.ToString()}");
                }
                #endregion

                foreach (var passenger in request.passengerDetails)
                {
                    var Res = new PassengerInfo();
                    Res.CreatedOn = DateTime.Now.ToUniversalTime();
                    Res.SessionId = request.sessionDetails.SessionId;
                    Res.FirstName = passenger.firstName;
                    Res.LastName = passenger.surName;
                    Res.PassengerType = passenger.type;
                    Res.PhoneNumber = passenger?.phone;
                    Res.Email = passenger?.email;
                    Res.DOB = passenger?.dob;
                    Res.IsLead = passenger?.isLeadPassenger;
                    Res.Flight = await _context.FlightsInfo.Where(e => e.FlightId == flightId).FirstOrDefaultAsync();
                    
                    await _context.PassengersInfo.AddAsync(Res);
                    await _context.SaveChangesAsync();
                }
              
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while saving Passenger Details {ex.Message.ToString()}");
            }


        }

        public async Task<List<PassengerInfo>>? GetPassengerInfo(string sessionId)
        {
            try
            {

                var pInfo = await _context.PassengersInfo.Where(e => e.SessionId == sessionId).ToListAsync();
                return pInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Get Passenger Details {ex.Message.ToString()}");
                return null;
            }


        }

        public async Task<FlightInfo>? GetFlightInfo(string sessionId)
        {
            try
            {

                var pInfo = await _context.FlightsInfo.Where(e => e.AmadeusSessionId == sessionId).FirstOrDefaultAsync();
                return pInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Get Flight info Details {ex.Message.ToString()}");
                return null;
            }


        }

        public async Task<BookingInfo>? GetBookingInfo(string sessionId)
        {
            try
            {

                var pInfo = await _context.BookingInfo.Where(e => e.SessionId == sessionId).FirstOrDefaultAsync();
                return pInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Get booking info Details {ex.Message.ToString()}");
                return null;
            }


        }
        public async Task<List<FlightMarkup>?> GetFlightMarkup()
        {
            try
            {

                return await _context.FlightMarkups.ToListAsync();
            }
            catch (Exception ex)
            {
             
                _logger.LogError($"Error return flights markup Result {ex.Message.ToString()}");
                return null;
            }


        }

        public async Task SaveBookingInfo( PnrCommitRequest request , string error , string pnrNumber)
        {

            try
            {
                var Res = new BookingInfo();
                Res.CreatedOn = DateTime.UtcNow;
                Res.SessionId = request.sessionDetails.SessionId;
                Res.FirstName = request.FirstName;
                Res.LastName = request.LastName;
                Res.PnrNumber = pnrNumber;
                Res.TotalAmount = request.TotalAmount;               
                Res.Error = error;
                Res.BookingRef = request.BookingRef;
                await _context.BookingInfo.AddAsync(Res);
                await _context.SaveChangesAsync();               

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while saving Passenger Details {ex.Message.ToString()}");
            }
        }

        public async Task<bool> UpdatePaymentStatus(string sessionId, string status)
        {

            try
            {
                var binfo = await _context.BookingInfo.Where(e => e.SessionId == sessionId).FirstOrDefaultAsync();
                if(binfo != null)
                {
                    binfo.PaymentStatus = status;
                    _context.BookingInfo.Update(binfo);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else { return false; }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Update Payment Status to bookinginfo {ex.Message.ToString()}");
                return false;
            }
        }

        public async Task<bool> UpdateEmailStatus (string sessionId,bool status)
        {

            try
            {
                var binfo = await _context.BookingInfo.Where(e => e.SessionId == sessionId).FirstOrDefaultAsync();
                if (binfo != null)
                {
                    binfo.SentEmail = status;
                    _context.BookingInfo.Update(binfo);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else { return false; }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Update Email Status to bookinginfo {ex.Message.ToString()}");
                return false;
            }
        }

        public async Task<bool> GetEmailStatus(string sessionId)
        {

            try
            {
                bool Res = false;
                var binfo = await _context.BookingInfo.Where(e => e.SessionId == sessionId).FirstOrDefaultAsync();
                if (binfo != null)
                {
                    Res = binfo.SentEmail != null ? binfo.SentEmail.Value  : false;
                    return Res;
                }
                else { return false; }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Update Email Status to bookinginfo {ex.Message.ToString()}");
                return false;
            }
        }

        public async Task<string> GetLastSessionId()
        {

            try
            {
                string Res = string.Empty;
                var binfo = await _context.BookingInfo.OrderByDescending(e => e.AutoId).FirstOrDefaultAsync();
                if (binfo != null)
                {
                    Res = binfo?.SessionId?.ToString();
                    return Res;
                }
                else { return Res; }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Get SessionID {ex.Message.ToString()}");
                return "";
            }
        }
    }
}
