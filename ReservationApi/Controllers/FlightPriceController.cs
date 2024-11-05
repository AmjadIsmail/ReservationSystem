using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.FlightPrice;
using ReservationSystem.Domain.Models.Soap.FlightPrice;
using ReservationSystem.Domain.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPriceController : ControllerBase
    {
        private IFlightPriceRepository _flightprice;
        private IAvailabilityRepository _availability;
        private readonly IMemoryCache _cache;
        public FlightPriceController(IFlightPriceRepository flightprice, IMemoryCache memoryCache, IAvailabilityRepository availability)
        {
            _flightprice = flightprice;
            _cache = memoryCache;
            _availability = availability;
        }

        [HttpGet]
        public string getvalues()
        {
            return "ok";
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FlightPriceMoelSoap? flightPriceRequest)
        {
            FlightPriceModel returnModel = new FlightPriceModel();
            var data = await _flightprice.GetFlightPrice(flightPriceRequest);
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = data.amadeusError == null;
            res.StatusCode = data.amadeusError == null ? 200 : 500;
            if (data.amadeusError != null)
            {
                res.Data = data.amadeusError;
                res.StatusCode = data.amadeusError.errorCode.Value;
            }
            else
            {
                res.Data = data;
            }
            res.Message = data?.amadeusError == null ? "Found Success: Total records:" + data.flightPrice.ToList().Count() : "Error";
            res.Response = data?.amadeusError == null ? "Success" : "Failed";
            return Ok(res);
        }

        [HttpPost("BestPrice")]
        public async Task<IActionResult> PostBestPrice([FromBody] FlightPriceMoelSoap? flightPriceRequest)
        {
            FlightPriceModel returnModel = new FlightPriceModel();
            var data = await _flightprice.GetFlightPriceWithBestPrice(flightPriceRequest);
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = data.amadeusError == null;
            res.StatusCode = data.amadeusError == null ? 200 : 500;
            if (data.amadeusError != null)
            {
                res.Data = data.amadeusError;
                res.StatusCode = data.amadeusError.errorCode.Value;
            }
            else
            {
                res.Data = data;
            }
            res.Message = data?.amadeusError == null ? "Found Success: Total records:" + data.flightPrice.ToList().Count() : "Error";
            res.Response = data?.amadeusError == null ? "Success" : "Failed";
            return Ok(res);
        }
    }
}