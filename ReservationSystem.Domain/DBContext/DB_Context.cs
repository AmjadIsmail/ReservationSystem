using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Domain.DB_Models;

namespace ReservationSystem.Domain.DBContext
{
    public class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options) : base(options)
        {
        }
       
        public static DB_Context Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DB_Context>();
            optionsBuilder.UseNpgsql(connectionString);

            return new DB_Context(optionsBuilder.Options);
        }

        public DbSet<SearchAvailabilityResults> availabilityResults { get; set; }
        public DbSet<FlightMarkup> flightMarkups { get; set; }
    }
}
