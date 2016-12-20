using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KaCake.Data.Models
{
    public class TaskGroup
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<TaskVariant> Variants { get; set; }
    }
}