using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.DBContext;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReservationSystem.Infrastructure.Repositories
{
    public class DBRepository : IDBRepository
    {
        private DB_Context _context;
        private readonly ILogger<DBRepository> _logger;
        public DBRepository(IConfiguration configuration, DB_Context context, ILogger<DBRepository> logger)
        {
           _context = context;
            _logger = logger;
        }

        public async Task SaveAvailabilityResult(string? requset , string? response , int totlResults)
        {
            try
            {
                var Res = new SearchAvailabilityResults();
                Res.created_on = DateTime.Now;
                Res.request = JsonDocument.Parse(requset).ToString();
                Res.response = JsonDocument.Parse(response).ToString();
                Res.total_results = totlResults;
                Res.user_id = 0;
                await _context.availabilityResults.AddAsync(Res);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while saving Search Result {ex.Message.ToString()}");
            }
          
           
        }
    }
}
