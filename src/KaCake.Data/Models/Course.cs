using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KaCake.Data.Models
{
    public class Course
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<CourseTeacher2> Teachers { get; set; }

        public ICollection<CourseEnrollment> Students { get; set; }

        public ICollection<TaskGroup> TaskGroups { get; set; }
    }
}
