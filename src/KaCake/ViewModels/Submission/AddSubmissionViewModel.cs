using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KaCake.ViewModels.Submission
{
    public class AddSubmissionViewModel
    {
        [Required]
        [HiddenInput]
        public int TaskVariantId { get; set; }

        [Required]
        [MinLength(1)]
        public IFormFileCollection Files { get; set; }
    }
}
