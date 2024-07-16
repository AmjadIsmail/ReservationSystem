using Microsoft.Extensions.Configuration;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReservationSystem.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using ReservationSystem.Domain.Models.Availability;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;


namespace ReservationSystem.Infrastructure.Repositories
{
    public class AvailabilityRepository  : IAvailabilityRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache _cache;
        public AvailabilityRepository(IConfiguration _configuration , IMemoryCache cache)
        {
            configuration = _configuration;
            _cache = cache;
        }
        public async Task<string> getToken()
        {
            try
            {
                string token;
                if (!_cache.TryGetValue("amadeusToken", out token))
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
                            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
                            _cache.Set("amadeusToken", accessToken, cacheEntryOptions);
                            Console.WriteLine("Response: " + responseContent);
                        }
                        else
                        {
                            Console.WriteLine("Error: " + response.StatusCode);
                        }
                    }
                    token = returnStr;
                }
               
                return token;
            }
            catch( Exception ex)
            {
                return "";
            }
          
        }

        public async Task<AvailabilityModel> GetAvailability(string token , AvailabilityRequest requestModel)
        {
            string amadeusRequest = generateRequest(requestModel);
            AvailabilityModel availabilities = new AvailabilityModel();
            if (!_cache.TryGetValue("amadeusRequest"+ amadeusRequest, out availabilities))
            {
                availabilities = new AvailabilityModel();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    var request = new HttpRequestMessage(HttpMethod.Get, amadeusRequest);
                    request.Headers.Add("Authorization", "Bearer " + token);

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                       // responseContent = responseContent.ToString().Replace("\"base\":", "\"base_amount\":");
                     
                        AvailabilityResult result = JsonConvert.DeserializeObject<AvailabilityResult>(responseContent);
                        availabilities.data = result.data;
                        if (result.data.Count > 0)
                        {
                            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
                            _cache.Set("amadeusRequest" + amadeusRequest, availabilities, cacheEntryOptions);
                        }
                        
                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        availabilities.amadeusError = new AmadeusResponseError();
                        availabilities.amadeusError.error = response.StatusCode.ToString();
                        availabilities.amadeusError.errorCode = 400;
                        if(response.StatusCode.ToString() == "Unauthorized")
                        {
                            _cache.Remove("amadeusToken");
                            availabilities.amadeusError.errorCode = 401;
                        }                     
                        var error = await response.Content.ReadAsStringAsync();
                        ErrorResponseAmadeus errorResponse = JsonConvert.DeserializeObject<ErrorResponseAmadeus>(error);

                        availabilities.amadeusError.error = response.StatusCode.ToString();
                        availabilities.amadeusError.error_details = errorResponse;
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
            }
               
            return availabilities;
        }
        private string generateRequest(AvailabilityRequest request)
        {
            string res = string.Empty;
            try
            {
                // https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=SYD&destinationLocationCode=BKK&departureDate=2024-07-02&returnDate=2024-07-22&adults=2&children=1&infants=1&travelClass=ECONOMY&nonStop=false&currencyCode=GBP&maxPrice=500&max=25
                // https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode=SYD&destinationLocationCode=BKK&departureDate=2024-07-02&returnDate=2024-07-22&adults=2&children=1&infants=1&travelClass=ECONOMY&nonStop=False&currencyCode=GBP&maxPrice=500&max=20
                var amadeusSettings = configuration.GetSection("Amadeus");
                var apiUrl = amadeusSettings["apiUrl"]+ "v2/shopping/flight-offers?";
                res = apiUrl + "originLocationCode=" + request.origin + "&destinationLocationCode=" + request.destination;
                res = res + "&departureDate=" + request.departureDate;
                if(!String.IsNullOrEmpty(request.returnDate))
                {
                    res = res + "&returnDate=" + request.returnDate;
                }
                res = res + "&adults=" + request.adults;
                if(request.children != null && request.children != 0)
                {
                    res = res + "&children=" + request.children;
                }
                if (request.infant != null && request.infant != 0)
                {
                    res = res + "&infants=" + request.infant;
                }
                if (!String.IsNullOrEmpty(request.travelClass))
                {
                    res = res + "&travelClass=" + request.travelClass;
                }
                if(request.nonStop != null)
                {
                    res = res + "&nonStop=" + request.nonStop.ToString().ToLower();
                }
                res = res + "&currencyCode=GBP";
                if(request.maxPrice != null)
                {
                    res = res + "&maxPrice=" + request.maxPrice;
                }
                if (request.maxFlights  != null)
                {
                    res = res + "&max=" + request.maxFlights;
                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }        
    }

}
