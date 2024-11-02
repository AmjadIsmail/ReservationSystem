using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.PnrQueue;
using ReservationSystem.Domain.Models.PNRRetrive;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueuePnrController : ControllerBase
    {
        private IPnrQueueRepository _repo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        public QueuePnrController(IPnrQueueRepository repo, IMemoryCache memoryCache, ICacheService cacheService)
        {
            _repo = repo;
            _cache = memoryCache;
            _cacheService = cacheService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PnrQueueRequest pnrRequest)
        {

            ApiResponse res = new ApiResponse();

            var data = await _repo.CreatePnrQueue(pnrRequest);

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
            }

            return Ok(res);

        }
    }
}
