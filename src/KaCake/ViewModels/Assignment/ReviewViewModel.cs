using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data.Models;
using KaCake.ViewModels.Submission;

namespace KaCake.ViewModels.Assignment
{
    public class ReviewViewModel
    {
        public string TaskGroupName { get; set; }
        public string TaskVariantName { get; set; }

        public ReviewStatus Status { get; set; }
        [Required]
        public double Score { get; set; }

        public string UserName { get; set; }

        public List<SubmissionViewModel> Submissions { get; set; }
        public int VaraintId { get; set; }
        public string UserId { get; set; }
    }
}
