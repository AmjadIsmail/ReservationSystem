using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.PnrCancel;
using ReservationSystem.Domain.Models.TicketCancel;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketCancelController : ControllerBase
    {
        private ITicketCancelRepository _repo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly IHelperRepository _helperRepository;
        public TicketCancelController(ITicketCancelRepository repo, IMemoryCache memoryCache, ICacheService cacheService,IHelperRepository helperRepository)
        {
            _repo = repo;
            _cache = memoryCache;
            _cacheService = cacheService;
            _helperRepository = helperRepository;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TicketCancelRequest pnrRequest)
        {

            ApiResponse res = new ApiResponse();

            var data = await _repo.CancelTicket(pnrRequest);

            res.IsSuccessful = data?.amadeusError == null ? true : false;
            res.StatusCode = data?.amadeusError == null ? 200 : 500;
            res.Message = data?.amadeusError == null ? "Success" : "Error";
            res.Response = data?.amadeusError == null ? "Success" : "Failed";
            if (data?.amadeusError != null)
            {
                res.Data = data?.amadeusError;
                res.StatusCode = data?.amadeusError?.errorCode.Value != 0 ? data.amadeusError.errorCode.Value : 500;
            }
            else
            {
                res.Data = data;
                await _helperRepository.Security_Signout(data.session);
            }
            
            return Ok(res);

        }
    }
}
