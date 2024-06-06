using Microsoft.EntityFrameworkCore;
using ReservationApi.Model;

namespace ReservationApi.DBContext
{
    public class EF_DataContext : DbContext
    {
        public EF_DataContext(DbContextOptions<EF_DataContext> options) : base(options) { }
        public static EF_DataContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EF_DataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new EF_DataContext(optionsBuilder.Options);
        }

        public DbSet<Animal> animals { get; set; }
    }
}
