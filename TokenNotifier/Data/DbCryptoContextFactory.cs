using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Data;

namespace TokenNotifier.Data
{
    public class DbCryptoContextactory : IDesignTimeDbContextFactory<DbCryptoContext>
    {
        public DbCryptoContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<DbContext>();
            var connectionString = configuration.GetConnectionString("TokenNotifierContext");
            builder.UseMySql(connectionString);
            return new DbCryptoContext(builder.Options);
        }
    }
}
