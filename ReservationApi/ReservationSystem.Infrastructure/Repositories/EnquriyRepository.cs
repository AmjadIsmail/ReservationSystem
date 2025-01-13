using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.DBLogs;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ReservationSystem.Domain.Models.Enquiry;
using ReservationSystem.Domain.DBContext;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class EnquriyRepository : IEnquriyRepository
    {
        private readonly IConfiguration configuration;
        private readonly IHelperRepository _helperRepository;
        private readonly IEmailService _emailService;
        private readonly DB_Context _dbRepository;
        public EnquriyRepository(IConfiguration _configuration, IMemoryCache cache, IEmailService emailService, ICacheService cacheService, DB_Context dBRepository)
        {
            configuration = _configuration;
            _emailService = emailService;
            _dbRepository = dBRepository;
        }

        public async Task<bool> SendEnquiry(EnquiryRequest requestModel)
        {
           try
            {
                #region Save Enquiry To Database
                Enquiry enquiry = new Enquiry
                {
                CreatedOn = DateTime.UtcNow,
                EmailAddress = requestModel.EmailAddress,
                FirstName = requestModel.FirstName ,
                LastName = requestModel.LastName,
                Message = requestModel.Message,
                PhoneNumber = requestModel.PhoneNumber,
                Source = requestModel.Source
                };
                await  _dbRepository.Enquiries.AddAsync(enquiry);
                await _dbRepository.SaveChangesAsync();
                #endregion

                #region Email Sending Region
                string MsgBoday = await _emailService.GetEnquiryTemplate(requestModel);
                string AdminEmail = "bookings@jaystravels.co.uk";
                if (configuration["EmailSettings:AdminEmail"] != null)
                {
                    AdminEmail = configuration["EmailSettings:AdminEmail"].ToString();
                }
                await _emailService.SendEmailAsync3(AdminEmail, "Enquiry From Customer - Source : " + requestModel.Source, MsgBoday);
                await _emailService.SendEmailAsync3(requestModel.EmailAddress, "Enquiry", MsgBoday);
                #endregion
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving request {ex.Message.ToString()}");
            }
            return false;
        }
    }
}
