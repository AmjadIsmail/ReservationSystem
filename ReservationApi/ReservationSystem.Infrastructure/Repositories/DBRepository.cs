using Microsoft.EntityFrameworkCore;
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
                Res.created_on = DateTime.Now.ToUniversalTime();
                Res.request = requset;
                Res.response = response;
                Res.total_results = totlResults;
                Res.user_id = 0;
                await _context.availabilityResults.AddAsync(Res);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while saving Search Result {ex.Message.ToString()}");
            }
          
           
        }

        public async Task<List<FlightMarkup>?> GetFlightMarkup()
        {
            try
            {

                return await _context.flightMarkups.ToListAsync();
            }
            catch (Exception ex)
            {
             
                _logger.LogError($"Error return flights markup Result {ex.Message.ToString()}");
                return null;
            }


        }
    }
}
