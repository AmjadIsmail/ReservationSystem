using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.FlightPrice;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Models.FlightOrder;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightOrderController : ControllerBase
    {
        private IFlightOrderRepository _flightorder;
        private IAvailabilityRepository _availability;
        private readonly IMemoryCache _cache;
        public FlightOrderController(IFlightOrderRepository flightorder, IMemoryCache memoryCache, IAvailabilityRepository availability)
        {
            _flightorder = flightorder;
            _cache = memoryCache;
            _availability = availability;
        }
       
        // POST api/<FlightOrderController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FlightOrderCreateRequestModel? flightorderRequst)
        {
           
            string Token = await _availability.getToken();
            var data = await _flightorder.CreateFlightOrder(Token, flightorderRequst);
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = true;
            res.StatusCode = 200;
            if (data.amadeusError != null)
            {
                res.Data = data.amadeusError;
                res.StatusCode = data.amadeusError.errorCode.Value;
            }
            else
            {
                res.Data = data;
            }
            return Ok(res);
        }


    }
}
