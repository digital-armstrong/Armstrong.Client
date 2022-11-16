using Armstrong.Client.Helpers;
using Armstrong.Client.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Armstrong.Client.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Channel>? Channels { get; set; }
        public DbSet<History>? Histories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var _host = EnvironmentHelper.GetEnvirovmentVariable("CLIENT_HOST");
            var _username = EnvironmentHelper.GetEnvirovmentVariable("CLIENT_USERNAME");
            var _pswd = EnvironmentHelper.GetEnvirovmentVariable("CLIENT_PSWD");
            var _database = EnvironmentHelper.GetEnvirovmentVariable("CLIENT_DATABASE");

            var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = _host,
                Username = _username,
                Password = _pswd,
                Database = _database
            };

            options.UseNpgsql(connectionStringBuilder.ConnectionString,
                options => options.EnableRetryOnFailure(maxRetryCount: 100));

            options.EnableSensitiveDataLogging();
            options.LogTo(message => Debug.WriteLine(message));
        }
    }
}
