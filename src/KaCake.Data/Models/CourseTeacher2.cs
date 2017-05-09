using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KaCake.Data.Models
{
    public class CourseTeacher2
    {
        [ForeignKey(nameof(Course))]
        [Column(Order = 0)]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey(nameof(Teacher))]
        [Column(Order = 1)]
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }

        // Who has appoint this teacher as a course teacher
        [ForeignKey(nameof(Appointer))]
        public string AppointerId { get; set; }
        public ApplicationUser Appointer { get; set; }
    }
}
