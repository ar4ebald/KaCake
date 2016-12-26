using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Mvc;

namespace KaCake.ViewModels.TaskVariant
{
    public class TaskVariantViewModel
    {
        public int Id { get; set; }

        [Required]
        [HiddenInput]
        public int TaskGroupId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public bool IsAssigned { get; set; }

        public string Description { get; set; }

        public int AssignmentsCount { get; set; }

        public ICollection<AssignmentViewModel> Assignments { get; set; }
    }
}