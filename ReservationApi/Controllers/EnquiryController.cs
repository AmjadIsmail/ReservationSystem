using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.Availability;
using ReservationSystem.Domain.Models.Enquiry;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiryController : ControllerBase
    {
        private IEnquriyRepository _repo;
       
        public EnquiryController(IEnquriyRepository repo)
        {
            _repo = repo;
        }

        //[Authorize]
        [HttpPost("sendenquiry")]
        public async Task<IActionResult> Post([FromBody] EnquiryRequest request)
        {

            ApiResponse res = new ApiResponse();
            var data = await _repo.SendEnquiry(request);
            res.IsSuccessful = data;
            res.StatusCode = data ? 200 : 500;
            res.Message = data ? "Send Email Success" : "Faile to send email";
            res.Response = data ? "Send Email Success" : "Faile to send email";
            res.Data = data;   
            return Ok(res);

        }

    }
}
