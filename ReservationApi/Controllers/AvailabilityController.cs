using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ReservationApi.Model;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Service;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private ITravelBoardSearchRepository  _availability;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHelperRepository _helperRepository;
        public AvailabilityController(ITravelBoardSearchRepository availability,IMemoryCache memoryCache,ICacheService cacheService ,IConfiguration configuration , IHelperRepository helperRepository)
        {
            _availability = availability;
            _cache = memoryCache;
            _cacheService = cacheService;
            _configuration = configuration;
            _helperRepository = helperRepository;
        }
              
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AvailabilityRequest availabilityRequest)
        {           
           
            ApiResponse res = new ApiResponse();
            
             var data = await _availability.GetAvailability( availabilityRequest);
              
                res.IsSuccessful = data?.amadeusError == null ? true : false;
                res.StatusCode = data?.amadeusError == null ? 200 : 500;
                res.Message = data?.amadeusError == null ? "Found Success: Total records:" + data.data.ToList().Count() : "Error";
                res.Response = data?.amadeusError == null ? "Success" : "Failed";
                if (data?.amadeusError != null)
                {
                    res.Data = data?.amadeusError;
                    res.StatusCode = data?.amadeusError?.errorCode.Value != 0 ? data.amadeusError.errorCode.Value : 500;
                }
                else
                {
                    res.Data = data.data;
                }               
            
            return Ok(res);

        }

        [HttpPost("signout")]
        public async Task<IActionResult> PostSignout([FromBody] HeaderSession request)
        {

            ApiResponse res = new ApiResponse();

             await _helperRepository.Security_Signout(request);
            res.IsSuccessful = true;
            res.StatusCode = 200;
            res.Message = "SingOut Success:";
            res.Response = "Success";
            res.Data = "SignOut Success";
            return Ok(res);
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
           
            string action = Environment.GetEnvironmentVariable(_configuration["AmadeusSoap:travelBoardSearchAction"]);
            ApiResponse res = new ApiResponse();            
            res.IsSuccessful = true;
            res.StatusCode = 200;
            res.Data = "API Service running properly , TravelboardSearch Url From EnvoirementVariable = " + action;
            res.Response = "Success";
            res.Message = "Success";
            return Ok(res);
        }

        [HttpGet("GetAirlines")]
        public async Task<IActionResult> GetAirlines()
        {

            ApiResponse res = new ApiResponse();
            var res2 = await _cacheService.CheckAirlines();
            res.IsSuccessful = true;
            res.StatusCode = 200;
            res.Data = res2;
            res.Response = "Success";
            res.Message = "Success";
            return Ok(res);
        }

        [HttpGet("clearCache")]
        public async Task<IActionResult> GetClearCache()
        {
           
            ApiResponse res = new ApiResponse();
            await _availability.ClearCache();
            res.IsSuccessful = true;
            res.StatusCode = 200;           
            res.Data = "Clear Success"; 
            return Ok(res);
        }


    }
}
