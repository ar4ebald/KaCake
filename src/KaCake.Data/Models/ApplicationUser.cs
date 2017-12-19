using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace KaCake.Data.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<CourseEnrollment> Courses { get; set; }
        public ICollection<Assignment> Assignments { get; set; }

        public ICollection<CourseTeacher> TeachingCourses { get; set; }

        public ICollection<Course> CreatedCourses { get; set; }
    }
}
