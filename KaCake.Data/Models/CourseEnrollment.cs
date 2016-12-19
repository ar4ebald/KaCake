using System;
using System.Collections.Generic;
using System.Text;

namespace KaCake.Data.Models
{
    public class CourseEnrollment
    {
        public Course Course { get; set; }
        public ApplicationUser User { get; set; }
    }
}
