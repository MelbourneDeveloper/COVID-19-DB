using Covid19DB.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covid19DB.Db
{
    public class Covid19DbContext : DbContext
    {
        #region Public Properties
        public DbSet<LocationDay> LocationDays { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Region> Regions { get; set; }
        #endregion

        #region Constructor
        public Covid19DbContext()
        {
            _ = Database.EnsureCreated();
        }
        #endregion

        #region Overrides
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new SqliteConnection("Data Source=Covid19Db.db");
            connection.Open();

            var command = connection.CreateCommand();

            //Create the database if it doesn't already exist
            command.CommandText = "PRAGMA foreign_keys = ON;";
            _ = command.ExecuteNonQuery();
            _ = optionsBuilder.UseSqlite(connection);

            base.OnConfiguring(optionsBuilder);
        }
        #endregion

    }
}
