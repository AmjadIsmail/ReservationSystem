using Microsoft.AspNetCore.Mvc;
using ReservationApi.Model;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        // GET: api/<CustomerController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ApiResponse response = new ApiResponse();

            List<Customer> custList = new List<Customer>();

            Customer c1 = new Customer();
            c1.customer_Id = 1;
            c1.customer_name = "Zain";
            

            custList.Add(c1);

            Customer c2 = new Customer();
            c2.customer_Id = 2;
            c2.customer_name = "Amjad";

            custList.Add(c2);

            response.Data = "Success to get customer list";
            response.IsSuccessful = true;
            response.Response = custList;
            response.StatusCode = (int)HttpStatusCode.OK;
            return Ok(response);
        }

        // GET api/<CustomerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CustomerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CustomerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CustomerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
