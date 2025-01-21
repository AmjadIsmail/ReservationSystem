using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ReservationSystem.Domain.Models.Payment;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using ReservationSystem.Domain.Models.ManualPayment;
using ReservationSystem.Domain.DBContext;
using ReservationApi.ReservationSystem.Domain.DB_Models;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Math;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        private readonly DB_Context _Context;
        public PaymentRepository(IConfiguration _configuration, IMemoryCache cache, IHelperRepository helperRepository , DB_Context Context)
        {
            configuration = _configuration;
            _cache = cache;
            _helperRepository = helperRepository;
            _Context = Context;
        }

        public async Task<PaymentResponse> GeneratePaymentRequest(PaymentRequest request)
        {
            PaymentResponse response = new PaymentResponse();
            try
            {
                var _settings = configuration.GetSection("EpdqSettings");
                string OrderId = Guid.NewGuid().ToString().Substring(0, 13);
                string strPw = string.Empty;    //_settings["ShaInPassphrase"]?.ToString();
                string pspid = string.Empty;
               if( Environment.GetEnvironmentVariable("PSPID") != null)                   
                {
                    pspid = Environment.GetEnvironmentVariable("PSPID").ToString();
                }
                else
                {
                    pspid = "jaystravelstest";
                }
               if (Environment.GetEnvironmentVariable("ShaInPassphrase") != null)
                {
                    strPw = Environment.GetEnvironmentVariable("ShaInPassphrase").ToString();
                }
                else
                {
                    strPw = "f3d1f70c-b1a0-48ba-816c-88fcccf72bf3";
                }
                var parameters = new Dictionary<string, string>
                {
                    { "ACCEPTURL", _settings["AcceptUrl"] },
                    { "AMOUNT", ((int)(request.Amount * 100)).ToString() },
                    { "CANCELURL", _settings["CancelUrl"] },                  
                    { "CURRENCY", request.Currency },
                    { "DECLINEURL", _settings["DeclineUrl"] },                  
                    { "EXCEPTIONURL", _settings["ExceptionUrl"] },
                    { "LANGUAGE", request.Language },
                    { "ORDERID",OrderId},
                    { "PSPID", pspid },
                 
                };
                string shaSignature = GenerateShaSignature(parameters,strPw);
                string paymentUrl = string.Empty;
                if(Environment.GetEnvironmentVariable("PaymentUrl") != null)
                {
                    paymentUrl = Environment.GetEnvironmentVariable("PaymentUrl")?.ToString();
                }
                else
                {
                    paymentUrl = "https://mdepayments.epdq.co.uk/ncol/test/orderstandard.asp";
                }
                parameters = new Dictionary<string, string>
                {
                    { "ACCEPTURL", _settings["AcceptUrl"] },
                    { "AMOUNT", ((int)(request.Amount * 100)).ToString() },
                    { "CANCELURL", _settings["CancelUrl"] },
                    { "CURRENCY", request.Currency },
                    { "DECLINEURL", _settings["DeclineUrl"] },
                    { "EXCEPTIONURL", _settings["ExceptionUrl"] },
                    { "LANGUAGE", request.Language },
                    { "ORDERID",OrderId},
                    { "PSPID", pspid },
                    { "SHASIGN", shaSignature },
                };
                response.Parameters = parameters;
                response.Url = paymentUrl; //_settings["PaymentUrl"];
                response.BookingRefNo = _helperRepository.GenerateReferenceNumber();
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error in Create Payment Request {ex.Message.ToString()}");
            }
            return response;
        }
        private string Sha1HasData(string data)
        {
            SHA1 Hasher = SHA1.Create();
            byte[] NCodetxt = Encoding.Default.GetBytes(data);
            byte[] HashedDataBytes = Hasher.ComputeHash(NCodetxt);
            StringBuilder HashedDataStringBuld = new StringBuilder();
            for(int i=0;i< HashedDataBytes.Length; i++)
            {
                HashedDataStringBuld.Append(HashedDataBytes[i].ToString("X2"));
            }
            return HashedDataStringBuld.ToString();
        }
      
        static string GenerateShaSignature(Dictionary<string, string> fields, string shaInPassphrase)
        {

            var sortedFields = fields
                .Where(field => !string.IsNullOrWhiteSpace(field.Value))
                .OrderBy(field => field.Key)
                .ToDictionary(field => field.Key.ToUpper(), field => field.Value);

            var concatenatedString = new StringBuilder();
            foreach (var field in sortedFields)
            {
                concatenatedString.Append($"{field.Key}={field.Value}").Append(shaInPassphrase);
            } 
            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString.ToString()));

                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }
        public async Task<bool> VerifyShaOutSignature(Dictionary<string, string> form)
        {
            try
            {
                var _settings = configuration.GetSection("EpdqSettings");
                string passphrase = _settings["ShaOutPassphrase"];
                var sortedParams = form
                                .Where(p => !string.IsNullOrEmpty(p.Value) && p.Key != "SHASIGN")
                                .OrderBy(p => p.Key);

                var sb = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    sb.Append(param.Key).Append('=').Append(param.Value).Append(passphrase);
                }
                using var sha256 = SHA256.Create();
                var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                var computedSignature = BitConverter.ToString(computedHash).Replace("-", string.Empty).ToUpper();

                return computedSignature == form.GetValueOrDefault("SHASIGN");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error while Verify Shah out Signature {ex.Message.ToString()}");
                return false;
            }
            
        }

        public async Task<PaymentResponse> GenerateManualPaymentRequest(PaymentRequest request)
        {
            PaymentResponse response = new PaymentResponse();
            try
            {
                var _settings = configuration.GetSection("EpdqSettings");
                string OrderId = Guid.NewGuid().ToString().Substring(0, 13);
                string strPw = string.Empty;//_settings["ShaInPassphrase"]?.ToString();
                string pspid = string.Empty;
                if (Environment.GetEnvironmentVariable("PSPID") != null)
                {
                    pspid = Environment.GetEnvironmentVariable("PSPID").ToString();
                }
                else
                {
                    pspid = "jaystravelstest";
                }
                if (Environment.GetEnvironmentVariable("ShaInPassphrase") != null)
                {
                    strPw = Environment.GetEnvironmentVariable("ShaInPassphrase").ToString();
                }
                else
                {
                    strPw = "f3d1f70c-b1a0-48ba-816c-88fcccf72bf3";
                }

                var parameters = new Dictionary<string, string>
            {
                    { "ACCEPTURL", _settings["AcceptUrlManual"] },
                    { "AMOUNT", ((int)(request.Amount * 100)).ToString() },
                    { "CANCELURL", _settings["CancelUrlManual"] },
                    { "CURRENCY", request.Currency },
                    { "DECLINEURL", _settings["DeclineUrlManual"] },
                    { "EXCEPTIONURL", _settings["ExceptionUrlManual"] },
                    { "LANGUAGE", request.Language },
                    { "ORDERID",OrderId},
                    { "PSPID", pspid },

            };
                string shaSignature = GenerateShaSignature(parameters, strPw);
                string paymentUrl = string.Empty;
                if (Environment.GetEnvironmentVariable("PaymentUrl") != null)
                {
                    paymentUrl = Environment.GetEnvironmentVariable("PaymentUrl")?.ToString();
                }
                else
                {
                    paymentUrl = "https://mdepayments.epdq.co.uk/ncol/test/orderstandard.asp";
                }
                parameters = new Dictionary<string, string>
                {
                    { "ACCEPTURL", _settings["AcceptUrlManual"] },
                    { "AMOUNT", ((int)(request.Amount * 100)).ToString() },
                    { "CANCELURL", _settings["CancelUrlManual"] },
                    { "CURRENCY", request.Currency },
                    { "DECLINEURL", _settings["DeclineUrlManual"] },
                    { "EXCEPTIONURL", _settings["ExceptionUrlManual"] },
                    { "LANGUAGE", request.Language },
                    { "ORDERID",OrderId},
                    { "PSPID", pspid },
                    { "SHASIGN", shaSignature },


                };
                response.Parameters = parameters;
                response.Url = paymentUrl;// _settings["PaymentUrl"];
                response.BookingRefNo = _helperRepository.GenerateReferenceNumber();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in Create Payment Request {ex.Message.ToString()}");
            }
            return response;
        }

        public async Task<bool> UpdateManualPaymentDetails(ManualPaymentCustomerDetails request)
        {
          
            try
            {
                 
                    ManualPayment payment = new ManualPayment();
                    payment.Address = request.Address;
                    payment.Amount = request?.Amount != null ? Convert.ToDecimal(request.Amount) : 0;
                    payment.City = request?.City;
                    payment.Country = request?.Country;
                    payment.CreatedOn = DateTime.Now;
                    payment.Email = request?.Email;
                    payment.FirstName = request?.FirstName;
                    payment.LastName = request?.LastName;
                    payment.PhoneNumber = request?.Phone;
                    payment.PostalCode = request?.Postal;
                    payment.PaymentStatus = request?.PaymentStatus;
                    await _Context.ManulPayments.AddAsync(payment);
                    await _Context.SaveChangesAsync();
                
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in Save Manual Payment Request {ex.Message.ToString()}");
                return false;
            }
            return true;
        }

    }
}
