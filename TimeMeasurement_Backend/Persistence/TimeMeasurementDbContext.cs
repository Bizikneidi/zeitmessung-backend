using Microsoft.EntityFrameworkCore;
using TimeMeasurement_Backend.Entities;

namespace TimeMeasurement_Backend.Persistence
{
    /// <summary>
    /// Context to connect a Database with C#
    /// </summary>
    public class TimeMeasurementDbContext : DbContext
    {
        /// <summary>
        /// table which stores every participant entity
        /// </summary>
        public DbSet<Participant> Participants { get; set; }

        /// <summary>
        /// table which stores every race entity
        /// </summary>
        public DbSet<Race> Races { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Set SQL Server Path
            optionsBuilder.UseSqlite("Data Source=./time_measurement.db");
        }
    }
}