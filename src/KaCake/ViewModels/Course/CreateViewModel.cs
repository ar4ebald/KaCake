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

        [Display(Name = "Users to add")]
        public IEnumerable<string> UsersToAdd { get; set; }

        [Display(Name = "Users to remove")]
        public IEnumerable<string> UsersToRemove { get; set; }
    }
}
