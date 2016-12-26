using System;
using System.Diagnostics;
using System.Linq;
using KaCake.Data.Models;
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

            //_context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Theory]
        [InlineData(20)]
        public void CoursesAreWorking(int count)
        {
            try
            {
                for (int i = 0; i < count; i++)
                    _context.Courses.Add(new Course() { Name = $"Course #{i}" });
                _context.SaveChanges();

                Assert.Equal(count, _context.Courses.Count());
            }
            finally
            {
                _context.Courses.RemoveRange(_context.Courses);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
