﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.FareCheck;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FareCheckController : ControllerBase
    {
        private FareCheckRepository _fareCheck;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        public FareCheckController(FareCheckRepository fareCheck, IMemoryCache memoryCache, ICacheService cacheService)
        {
            _fareCheck = fareCheck;
            _cache = memoryCache;
            _cacheService = cacheService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FareCheckModel _request)
        {
            // string Token = await _availability.getToken();
            ApiResponse res = new ApiResponse();

            var data = await _fareCheck.FareCheckRequest(_request);

            res.IsSuccessful = true;
            res.StatusCode = 200;
            if (data.amadeusError != null)
            {
                res.Data = data.amadeusError;
                res.StatusCode = data.amadeusError.errorCode.Value;
            }
            else
            {
                res.Data = data.data;
            }

            return Ok(res);

        }
    }
}