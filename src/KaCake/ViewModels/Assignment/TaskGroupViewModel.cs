using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using KaCake.ViewModels.TaskVariant;

namespace KaCake.ViewModels.Assignment
{
    public class TaskGroupViewModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required]
        [HiddenInput]
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public IList<TaskVariantViewModel> Variants { get; set; }
    }
}