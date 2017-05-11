using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace KaCake.ViewModels.TaskVariant
{
    public class TaskVariantViewModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required]
        [HiddenInput]
        public int TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }

        public int CourseId { get; set; }
        public string CourseName { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }


        public bool IsAssigned { get; set; }


        public int AssignmentsCount { get; set; }

        public ICollection<AssignmentViewModel> Assignments { get; set; }

        public IFormFile TesterArchive { get; set; }
    }
}