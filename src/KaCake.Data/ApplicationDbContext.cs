using KaCake.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KaCake.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Course> Courses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Course enrollment configuration as many-to-many relation
            builder.Entity<CourseEnrollment>()
                .HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Students)
                .IsRequired();
            builder.Entity<CourseEnrollment>()
                .HasOne(enrollment => enrollment.User)
                .WithMany(user => user.Courses)
                .IsRequired();
            builder.Entity<CourseEnrollment>()
                .HasKey(enrollment => new { CourseId = enrollment.Course.Id, UserId = enrollment.User.Id });


        }
    }
}
