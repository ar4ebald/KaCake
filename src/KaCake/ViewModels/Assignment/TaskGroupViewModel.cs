using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KaCake.ViewModels.TaskVariant;
using Microsoft.AspNetCore.Mvc;

namespace KaCake.ViewModels.Assignment
{
    public class TaskGroupViewModel
    {
        public int Id { get; set; }

        [Required]
        [HiddenInput]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public IList<TaskVariantViewModel> Variants { get; set; }
    }
}