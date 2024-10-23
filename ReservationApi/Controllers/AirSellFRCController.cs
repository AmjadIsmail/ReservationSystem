using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.AirSellFromRecommendation;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirSellFRCController : ControllerBase
    {

        private IAirSellRepository _airselRepo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        public AirSellFRCController(IAirSellRepository airselRepo, IMemoryCache memoryCache, ICacheService cacheService)
        {
            _airselRepo = airselRepo;
            _cache = memoryCache;
            _cacheService = cacheService;
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AirSellFromRecommendationRequest airSellRequest)
        {

            ApiResponse res = new ApiResponse();

            var data = await _airselRepo.GetAirSellRecommendation(airSellRequest);

            res.IsSuccessful = data?.amadeusError == null ? true : false;
            res.StatusCode = data?.amadeusError == null ? 200 : 500;
            res.Message = data?.amadeusError == null ? "Found Success: Total records:" + data.airSellResponse.ToList().Count() : "Error";
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