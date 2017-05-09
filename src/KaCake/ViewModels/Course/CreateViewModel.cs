using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KaCake.ViewModels.Course
{
    public class CreateViewModel
    {
        [HiddenInput]
        public int? Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Teachers to add")]
        public IEnumerable<string> TeachersToAdd { get; set; }

        [Display(Name = "Teachers to remove")]
        public IEnumerable<string> TeachersToRemove { get; set; }
    }
}
