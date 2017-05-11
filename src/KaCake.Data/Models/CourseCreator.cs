using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KaCake.Data.Models
{
    public class CourseCreator
    {
        [ForeignKey(nameof(User))]
        [Column(Order = 1)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(Course))]
        [Column(Order = 2)]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
