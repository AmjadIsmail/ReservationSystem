﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.FlightPrice;
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
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FlightOffer? flightofferRequst)
        {
            FlightPriceModel flightPriceRequest = new FlightPriceModel();
            flightPriceRequest.flightOffers = new FlightOffer();
            flightPriceRequest.flightOffers = flightofferRequst;
            string Token = await _availability.getToken();
            var data = await _flightprice.GetFlightPrice(Token, flightPriceRequest);
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
