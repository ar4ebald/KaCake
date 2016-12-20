using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KaCake.Data.Models
{
    public class TaskVariant
    {
        public int Id { get; set; }

        public int TaskGroupId { get; set; }
        public TaskGroup TaskGroup { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Assignment> Assignments { get; set; }
    }
}
