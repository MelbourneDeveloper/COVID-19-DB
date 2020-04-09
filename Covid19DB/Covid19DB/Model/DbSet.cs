using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Covid19DB.Entities
{
    public class Covid19DbContext : DbContext
    {
        public DbSet<Day> Days { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Region> Regions { get; set; }

        public Covid19DbContext()
        {
            //TODO: Make async
            Database.EnsureCreated();
        }

        /// <summary>
        /// Configure context to use Sqlite
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new SqliteConnection($"Data Source=Covid19Db.db");
            connection.Open();

            var command = connection.CreateCommand();

            //Create the database if it doesn't already exist
            command.CommandText = $"PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            optionsBuilder.UseSqlite(connection);

            base.OnConfiguring(optionsBuilder);
        }

    }
}
