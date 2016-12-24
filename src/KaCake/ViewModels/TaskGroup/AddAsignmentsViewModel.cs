using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KaCake.ViewModels.TaskGroup
{
    public class AddAsignmentsViewModel
    {
        public string TaskVariantName { get; set; }

        [Required]
        [HiddenInput]
        public int TaskVariantId { get; set; }

        [Required]
        [Display(Name = "Reviewer")]
        public string ReviewerId { get; set; }

        [Required]
        [Display(Name = "Deadline")]
        [DisplayFormat(DataFormatString = "DD.MM.YYYY HH:mm")]
        public DateTime DeadlineUtc { get; set; }

        [Display(Name = "Users to add")]
        public IEnumerable<string> UsersToAdd { get; set; }

        [Display(Name = "Users to remove")]
        public IEnumerable<string> UsersToRemove { get; set; }
    }
}
