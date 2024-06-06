using Microsoft.Extensions.Configuration;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models;


namespace ReservationSystem.Infrastructure.Repositories
{
    public class Availability  : IAvailability
    {
        private readonly IConfiguration configuration;
        public Availability(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<string> getToken()
        {
            string returnStr = "";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                var amadeusSettings = configuration.GetSection("Amadeus");

                var grantType = amadeusSettings["grant_type"];
                var clientId = amadeusSettings["client_id"];
                var clientSecret = amadeusSettings["client_secret"];
                var tokenurl = amadeusSettings["tokenUrl"];
                var formData = new Dictionary<string, string>
            {
                { "grant_type", grantType },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

                var request = new HttpRequestMessage(HttpMethod.Post, tokenurl);

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

        public async Task<List<AvailabilityModel>> GetAvailability(string token)
        {
           
            List<AvailabilityModel> availabilities = new List<AvailabilityModel>();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");


                var request = new HttpRequestMessage(HttpMethod.Get, "https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=ORL&destinationLocationCode=BKK&departureDate=2024-11-01&adults=1&max=20");
                request.Headers.Add("Authorization", "Bearer " + token);
                // request.Content = new FormUrlEncodedContent(formData);

                var response = await httpClient.SendAsync(request);

                // 6. Handle the response
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

                    for (int i = 0; i < 10; i++)
                    {
                        AvailabilityModel availability = new AvailabilityModel();
                        availability.id = 1; // data.id
                        availability.source = "GDS";// data.GDS
                        availabilities.Add(availability);

                    }
                    // 7. Extract the access_token
                    // string data = jsonResponse;
                    //returnStr = data;
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
