using Microsoft.EntityFrameworkCore;

namespace TimeMeasurement_Backend.Persistence
{
    /// <summary>
    /// Context to connect Database with C#
    /// </summary>
    public class TimeMeasurementDbContext : DbContext
    {
        //TODO add DbSet<...>

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Set SQL Server Path
            optionsBuilder.UseSqlServer(""); //TODO set sql server
        }
    }
}
