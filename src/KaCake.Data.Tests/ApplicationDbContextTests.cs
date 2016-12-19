using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace KaCake.Data.Tests
{
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDbContextTests()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            Debug.WriteLine(configuration.GetConnectionString("DefaultConnection"));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            _context = new ApplicationDbContext(options.Options);

            _context.Database.EnsureCreated();
        }

        [Fact]
        public void Test1()
        {
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
