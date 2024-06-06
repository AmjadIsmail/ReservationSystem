using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using ReservationApi.Model;
using System.Data;
using System.Text.Json;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalController : ControllerBase
    {
        // GET: api/<AnimalController>
        private readonly string _connectionString;

        public AnimalController( IConfiguration configuration)
        {
        
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var animalList = await GetAnimal();
            ApiResponse res = new ApiResponse();
            res.IsSuccessful = true;
            res.Data = animalList;
            return Ok(res);
        }

        private async Task<List<Animal>> GetAnimal()
        {
            List<Animal> returnAnimals = new List<Animal>();

            try
            {
                HttpClient httpClient = new HttpClient();
                List<Animal> animals = new List<Animal>();
                HttpResponseMessage response = await httpClient.GetAsync("https://freetestapi.com/api/v1/animals");
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JToken jToken = JToken.Parse(responseBody);
                    JObject[] items = jToken.ToObject<JObject[]>();
                    foreach (var item in items)
                    {
                        Animal obj = new Animal();
                        obj.id = Convert.ToInt16(item["id"]);
                        obj.name = Convert.ToString(item["name"]);
                        obj.species = Convert.ToString(item["spices"]);
                        obj.family = Convert.ToString(item["family"]);
                        obj.habitat = Convert.ToString(item["habitat"]);
                        obj.place_of_found = Convert.ToString(item["place_of_found"]);
                        obj.diet = Convert.ToString(item["diet"]);
                        obj.description = Convert.ToString(item["description"]);
                        obj.weight_kg = Convert.ToDecimal(item["weight_kg"]);
                        obj.height_cm = Convert.ToDecimal(item["height_cm"]);
                        obj.image = Convert.ToString(item["image"]);
                        animals.Add(obj);
                    }
                }


                return animals;
            }
            catch(Exception ex)
            {

            }
           

            return returnAnimals;
        }
        // GET api/<AnimalController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AnimalController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<AnimalModel> animals)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    foreach (var animal in animals)
                    {
                        string query = string.Empty;
                        query = @"INSERT INTO Animal
                               (id
                               ,name
                               ,species
                               ,family
                               ,habitat
                              )
                         VALUES ("+animal.id+",'"+animal.name+"','"+animal.species+"','"+animal.family+"','"+animal.habitat+"')";
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                        catch(Exception ex)
                        {
                            Console.Write(ex.Message);
                        }
                       
                    }
                   
                    /*
                      ,place_of_found
                               ,diet
                               ,description
                               ,weight_kg
                               ,height_cm
                               ,image */

                   
                    
                }
            }
            return Ok("sucess");


        }

        // PUT api/<AnimalController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AnimalController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
