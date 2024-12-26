using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.Payment;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IPaymentRepository _repo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHelperRepository _helperRepository;
        public PaymentController(IPaymentRepository repository, IMemoryCache memoryCache, ICacheService cacheService, IConfiguration configuration, IHelperRepository helperRepository)
        {
            _repo = repository;
            _cache = memoryCache;
            _cacheService = cacheService;
            _configuration = configuration;
            _helperRepository = helperRepository;
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
