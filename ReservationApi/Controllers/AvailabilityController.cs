﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ReservationApi.Model;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Repositories;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private IAvailabilityRepository _availability;
        private readonly IMemoryCache _cache;
        public AvailabilityController(IAvailabilityRepository availability,IMemoryCache memoryCache)
        {
            _availability = availability;
            _cache = memoryCache;
        }
              
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AvailabilityRequest availabilityRequest)
        {           
            string Token = await _availability.getToken();
            ApiResponse res = new ApiResponse();
            if (Token != "")
            {
                var data = await _availability.GetAvailability(Token, availabilityRequest);
              
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
            }
            else
            {
                res.Data = "";
                res.IsSuccessful = false;
                res.Message = "getting token error";
                
            }
            return Ok(res);

        }

       

       
    }
}
