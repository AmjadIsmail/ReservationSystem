using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReservationApi.Model;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.FOP;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FOPController : ControllerBase
    {
        private IFopRepository _Repo;
        private ICacheService _cacheService;
        private readonly IMemoryCache _cache;
        public FOPController(IFopRepository Repo, IMemoryCache memoryCache, ICacheService cacheService)
        {
            _Repo = Repo;
            _cache = memoryCache;
            _cacheService = cacheService;
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FopRequest req)
        {

            ApiResponse res = new ApiResponse();

            var data = await _Repo.CreateFOP(req);

            res.IsSuccessful = data?.amadeusError == null ? true : false;
            res.StatusCode = data?.amadeusError == null ? 200 : 500;
            res.Message = data?.amadeusError == null ? "Found Success:" : "Error";
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
