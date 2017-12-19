using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KaCake.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KaCake.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<TaskGroup> TaskGroups { get; set; }
        public DbSet<TaskVariant> TaskVariants { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=NewKaCakeDatabase;Integrated Security=True");
            }
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
                .HasKey(enrollment => new { enrollment.CourseId, enrollment.UserId });

            // Course to TaskGroup relation as one-to-many
            builder.Entity<Course>()
                .HasMany(course => course.TaskGroups)
                .WithOne(taskGroup => taskGroup.Course)
                .HasForeignKey(taskGroup => taskGroup.CourseId)
                .IsRequired();

            // TaskGroup to TaskVariant relation as one-to-many
            builder.Entity<TaskGroup>()
                .HasMany(taskGroup => taskGroup.Variants)
                .WithOne(taskVariant => taskVariant.TaskGroup)
                .HasForeignKey(taskVariant => taskVariant.TaskGroupId)
                .IsRequired();

            // TaskVariant to Assignment relation as one-to-many
            builder.Entity<TaskVariant>()
                .HasMany(taskVariant => taskVariant.Assignments)
                .WithOne(taskVariant => taskVariant.TaskVariant)
                .HasForeignKey(taskVariant => taskVariant.TaskVariantId)
                .IsRequired();

            // ApplicationUser to Assignment relation as one-to-many
            builder.Entity<ApplicationUser>()
                .HasMany(applicationUser => applicationUser.Assignments)
                .WithOne(assignment => assignment.User)
                .HasForeignKey(assignment => assignment.UserId)
                .IsRequired();

            // Assignment composite key definition
            builder.Entity<Assignment>()
                .HasKey(assignment => new { assignment.TaskVariantId, assignment.UserId });

            // Assignment to Submission relation as one-to-many
            builder.Entity<Assignment>()
                .HasMany(assignment => assignment.Submissions)
                .WithOne(submission => submission.Assignment)
                .IsRequired();

            // CourseTeacher key definition
            builder.Entity<CourseTeacher>()
                .HasKey(courseTeacher => new { courseTeacher.CourseId, courseTeacher.TeacherId });

            builder.Entity<CourseTeacher>()
                .HasMany(appointer => appointer.AppointedTeachers)
                .WithOne(teacher => teacher.Appointer)
                .HasForeignKey(teacher => new { teacher.CourseId, teacher.AppointerId })
                .IsRequired();

            // Course to CourseTeacher as one-to-many
            builder.Entity<Course>()
                .HasMany(course => course.Teachers)
                .WithOne(teacher => teacher.Course)
                .HasForeignKey(teacher => teacher.CourseId)
                .IsRequired();

            // ApplicationUser to CourseTeacher as one-to-many relation
            builder.Entity<ApplicationUser>()
                .HasMany(user => user.TeachingCourses)
                .WithOne(teacher => teacher.Teacher)
                .HasForeignKey(teacher => teacher.TeacherId)
                .IsRequired();
            
            builder.Entity<Course>()
                .HasOne(course => course.Creator)
                .WithMany(creator => creator.CreatedCourses)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(user => user.CreatedCourses)
                .WithOne(course => course.Creator)
                .IsRequired();
            

        }
    }
}
