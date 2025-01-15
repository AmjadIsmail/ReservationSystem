using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using ReservationSystem.Domain.Models.Email;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PNRController : ControllerBase
    {
        private IAddPnrMultiRepository _Repo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        private readonly IDBRepository _dBRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public PNRController(IAddPnrMultiRepository Repo, IMemoryCache memoryCache, ICacheService cacheService , IHelperRepository helperRepository , IDBRepository dBRepository , IEmailService emailService , IConfiguration configuration)
        {
            _Repo = Repo;
            _cache = memoryCache;
            _cacheService = cacheService;
            _helperRepository = helperRepository;
            _dBRepository = dBRepository;
            _emailService = emailService;
            _configuration = configuration;
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddPnrMultiRequset airSellRequest)
        {

            ApiResponse res = new ApiResponse();

            var data = await _Repo.AddPnrMulti(airSellRequest);

            res.IsSuccessful = data?.amadeusError == null ? true : false;
            res.StatusCode = data?.amadeusError == null ? 200 : 500;
            res.Message = data?.amadeusError == null ? "Found Success:" : "Error";
            res.Response = data?.amadeusError == null ? "Success" : "Failed";
            if (data?.amadeusError != null)
            {
                res.Data = data?.amadeusError;
                res.StatusCode = data?.amadeusError?.errorCode.Value != 0 ? data.amadeusError.errorCode.Value : 500;
            }
            else
            {
                res.Data = data;
            }

            return Ok(res);

        }
        
        [HttpPost("CommitPnr")]
        public async Task<IActionResult> CommitPnr([FromBody] PnrCommitRequest request)
        {

            ApiResponse res = new ApiResponse();

            var data = await _Repo.CommitPNR(request);

            res.IsSuccessful = data?.amadeusError == null ? true : false;
            res.StatusCode = data?.amadeusError == null ? 200 : 500;
            res.Message = data?.amadeusError == null ? "Found Success:" : "Error";
            res.Response = data?.amadeusError == null ? "Success" : "Failed";
            if (data?.amadeusError != null)
            {
                res.Data = data?.amadeusError;
                res.StatusCode = data?.amadeusError?.errorCode.Value != 0 ? data.amadeusError.errorCode.Value : 500;
            }
            else
            {
                res.Data = data;
                await _helperRepository.Security_Signout(request.sessionDetails);
            }

            return Ok(res);

        }

        [HttpPost("UpdatePaymentStatus")]
        public async Task<IActionResult> UpdatePaymentStatus([FromBody] UpdatePaymentStatus request)
        {

            #region Email Region
            try
            {
                if (String.IsNullOrEmpty(request?.SessionId))
                {
                    request.SessionId = await _dBRepository.GetLastSessionId();
                }
                var emailSent = await _dBRepository.GetEmailStatus(request?.SessionId);
                if (!emailSent)
                {
                    var emailBody = await _emailService.GetBookingSuccessTemplate(request?.SessionId, "Confirmed", request.PaymentStatus);
                    string subject = request?.PaymentStatus == "Success" ? "Reservation Success" : "Reservation Success with Payment Failed";
                    var pinfo = await _dBRepository.GetPassengerInfo(request?.SessionId);
                    string ToemailAddress = pinfo.Where(e => e.IsLead == true).FirstOrDefault()?.Email;
                    await _dBRepository.UpdateEmailStatus(request?.SessionId, true);
                    await _emailService.SendEmailAsync3(ToemailAddress, subject, emailBody);
                    var AdminEmail = _configuration["EmailSettings:AdminEmail"];
                    var AdminEmailBody = await _emailService.GetBookingSuccessTemplateForAdmin(request?.SessionId, request.PaymentStatus);
                    await _emailService.SendEmailAsync3(AdminEmail, "Admin-Portal " + subject, AdminEmailBody);
                }
               


            }
            catch (Exception ex)
            {

            }
            #endregion
            ApiResponse res = new ApiResponse();

             var data = await _Repo.UpdatePaymentStatusInBookingInfo(request);

            res.IsSuccessful = data ;
            res.StatusCode = 200;
            res.Message = data ? " Success:" : "Failed";
            res.Response = data ? "Success" : "Failed";           
            return Ok(res);

        }

    }
}
