using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReservationApi.Model;
using ReservationSystem.Domain.Models;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        // GET: api/<AvailabilityController>
        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string Token = await getToken();

           var data = await  getAvailability(Token);
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = true;
            res.Data = data;
            return Ok(res);
        }

        // GET api/<AvailabilityController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AvailabilityController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AvailabilityController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AvailabilityController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private async Task<string> getToken()
        {
            string returnStr = "";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                var formData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", "qqbJYl1jIuNmoGCkIeMLaSDbO3Slek4v" },
                { "client_secret", "EQnxMekrLGmgNJmA" }
            };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v1/security/oauth2/token");
                               
                request.Content = new FormUrlEncodedContent(formData);
               
                var response = await httpClient.SendAsync(request);

                // 6. Handle the response
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

                    // 7. Extract the access_token
                    string accessToken = jsonResponse.access_token;
                    returnStr = accessToken;
                    Console.WriteLine("Response: " + responseContent);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }
            return returnStr;
        }

        private async Task<Availability> getAvailability(string token)
        {
            string returnStr = "";
            Availability availabilities = new Availability();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                         

                var request = new HttpRequestMessage(HttpMethod.Get, "https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=ORL&destinationLocationCode=BKK&departureDate=2024-11-01&adults=1&max=20");
                request.Headers.Add("Authorization", "Bearer "+token);                
                var response = await httpClient.SendAsync(request);
               
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                 
                    AvailabilityResult result = JsonConvert.DeserializeObject<AvailabilityResult>(responseContent);                    
                    availabilities.data = result.data;
                    Console.WriteLine("Response: " + responseContent);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }
            return availabilities;
        }
    }
}
