using Microsoft.EntityFrameworkCore;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Entities.Constraints;

namespace TimeMeasurement_Backend.Persistence
{
    /// <summary>
    /// Context to connect Database with C#
    /// </summary>
    public class TimeMeasurementDbContext : DbContext
    {
        /// <summary>
        /// Mapping to table, storing every Participant entity
        /// </summary>
        public DbSet<Participant> Participants { get; set; }

        /// <summary>
        /// Mapping to table, storing every Time entity
        /// </summary>
        public DbSet<Time> Times { get; set; }
        
        /// <summary>
        /// Mapping to table, storing every Participant with its Time entity
        /// </summary>
        public DbSet<Runner> Timetable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Set SQL Server Path
            optionsBuilder.UseSqlite("Data Source=./time_measurement.db");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=(localdb)\\MSSQLLocalDB;" +
            //    "Initial Catalog=TimeMeasurementDb;" +
            //    "Integrated Security=True;" +
            //    "Pooling=True"
            //);
        }
    }
}