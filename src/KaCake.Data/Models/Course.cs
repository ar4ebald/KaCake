using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KaCake.Data.Models
{
    public class Course
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey(nameof(Creator))]
        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }

        public ICollection<CourseTeacher> Teachers { get; set; }

        public ICollection<CourseEnrollment> Students { get; set; }

        public ICollection<TaskGroup> TaskGroups { get; set; }
    }
}
