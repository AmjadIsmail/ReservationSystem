using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.ManualPayment;
using ReservationSystem.Domain.Models.Payment;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _repo;
        private readonly ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHelperRepository _helperRepository;
        private readonly IEmailService _emailService;
        public PaymentController(IPaymentRepository repository, IMemoryCache memoryCache, ICacheService cacheService, IConfiguration configuration, IHelperRepository helperRepository , IEmailService emailservice)
        {
            _repo = repository;
            _cache = memoryCache;
            _cacheService = cacheService;
            _configuration = configuration;
            _helperRepository = helperRepository;
            _emailService = emailservice;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePaymentRequest([FromBody] PaymentRequest request)          
        {
            ApiResponse res = new ApiResponse();
            var data = await _repo.GeneratePaymentRequest(request);
            res.IsSuccessful = data?.Error == null ? true : false;
            res.StatusCode = data?.Error == null ? 200 : 500;
            res.Message = data?.Error == null ? "Found Success:" :  "Error";
            res.Response = data?.Error == null ? "Success" : "Failed";
            if (data?.Error != null)
            {
                res.Data = data?.Error;
                res.StatusCode = 500;
            }
            else
            {
                res.Data = data;
            }
            return Ok(res);

        }

        [HttpPost("generatemanualpayment")]
        public async Task<IActionResult> GeneratePaymentRequestManual([FromBody] PaymentRequest request)
        {
            ApiResponse res = new ApiResponse();
            var data = await _repo.GenerateManualPaymentRequest(request);
            res.IsSuccessful = data?.Error == null ? true : false;
            res.StatusCode = data?.Error == null ? 200 : 500;
            res.Message = data?.Error == null ? "Found Success:" : "Error";
            res.Response = data?.Error == null ? "Success" : "Failed";
            if (data?.Error != null)
            {
                res.Data = data?.Error;
                res.StatusCode = 500;
            }
            else
            {
                res.Data = data;
            }
            return Ok(res);

        }

        [HttpPost("UpdateManualPayment")]
        public async Task<IActionResult> UpdateManualPayment([FromBody] ManualPaymentCustomerDetails request)
        {

            #region
            bool update = await _repo.UpdateManualPaymentDetails(request);
            #endregion
            #region Email Region
            try
            {
            // var emailBody = await _emailService.GetBookingSuccessTemplate(request?.SessionId, "Confirmed", request.PaymentStatus);
            //string subject = request?.PaymentStatus == "Success" ? "Reservation Success" : "Reservation Success with Payment Failed";
            //var pinfo = await _dBRepository.GetPassengerInfo(request?.SessionId);
            //string ToemailAddress = pinfo.Where(e => e.IsLead == true).FirstOrDefault()?.Email;
            //await _emailService.SendEmailAsync3(ToemailAddress, subject, emailBody);             
            }
            catch (Exception ex)
            {

            }
            #endregion
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = update;
            res.StatusCode = 200;
            res.Message = update ? " Success:" : "Failed";
            res.Response = update ? "Success" : "Failed";
            return Ok(res);

        }


        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback([FromForm] Dictionary<string, string> form)
        {
            // Verify SHA-OUT signature
            var check = await _repo.VerifyShaOutSignature(form);
            if (!check)
            {
                return BadRequest("Invalid SHA signature");
            }

            // Extract payment details
            var orderId = form.GetValueOrDefault("ORDERID");
            var status = form.GetValueOrDefault("STATUS");
            var paymentId = form.GetValueOrDefault("PAYID");
            var amount = form.GetValueOrDefault("AMOUNT");
            var currency = form.GetValueOrDefault("CURRENCY");

            // Determine the payment outcome
            if (status == "9") // Payment successful
            {
                // Redirect to success
                return Redirect($"https://yourfrontend.com/payment/success?orderId={orderId}&amount={amount}&currency={currency}");
            }
            else
            {
                // Redirect to failure
                return Redirect($"https://yourfrontend.com/payment/failure?orderId={orderId}&status={status}");
            }
        }

        [HttpPost("Success")]
        public IActionResult Success([FromForm] Dictionary<string, string> form)
        {
            // Verify SHA-OUT signature and handle success
            return Ok("Payment Successful");
        }

        [HttpPost("Decline")]
        public IActionResult Decline([FromForm] Dictionary<string, string> form)
        {
            // Handle declined payment
            return Ok("Payment Declined");
        }

        [HttpPost("Canceled")]
        public IActionResult Canceled([FromForm] Dictionary<string, string> form)
        {
            // Handle canceled payment
            return Ok("Payment Canceled");
        }

      

    }
}
