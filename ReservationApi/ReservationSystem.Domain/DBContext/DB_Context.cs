using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using ReservationApi.ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.DB_Models;
using ReservationSystem.Domain.Models.AddPnrMulti;
using ReservationSystem.Domain.Models.Enquiry;

namespace ReservationSystem.Domain.DBContext
{
    public class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
          
        }
  
    public DbSet<SearchAvailabilityResults> AvailabilityResults { get; set; }
        public DbSet<FlightMarkup> FlightMarkups { get; set; }

        public DbSet<ReservationFlow> ReservationFlow { get; set; }

        public DbSet<FlightInfo> FlightsInfo { get; set; }
        public DbSet<PassengerInfo> PassengersInfo { get; set; }

        public DbSet<BookingInfo> BookingInfo { get; set; }

        public DbSet<Users> Users { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }

        public DbSet<ManualPayment> ManulPayments { get; set; }

        public DbSet<MarketingSource> MarketingSources { get; set; }
        public DbSet<GDS> GDS { get; set; }

        public DbSet<DayName> DayName { get; set; }
       
        public DbSet<FareType> FareTypes { get; set; }

        public DbSet<ApplyMarkup> Markups { get; set; }

        public DbSet<MarkupDay> MarkupDay { get; set; }

        public DbSet<MarkupGDS> MarkupGds { get; set; }

        public DbSet<MarkupFareType> MarkupFareTypes { get; set; }

        public DbSet<MarkupMarketingSource> MarkupMarketingSources { get; set; }

        public DbSet<MarkupJournyType> MarkupJournyTypes { get; set; }

        public DbSet<JourneyType> JourneyTypes { get; set; }
    }
}
