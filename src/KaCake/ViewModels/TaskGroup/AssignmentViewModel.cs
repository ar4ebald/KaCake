using System.Collections.Generic;
using KaCake.Data.Models;
using KaCake.ViewModels.Submission;
using KaCake.ViewModels.TaskVariant;

namespace KaCake.ViewModels.TaskGroup
{
    public class AssignmentViewModel
    {
        public int TaskVariantId { get; set; }

        public TaskVariantViewModel TaskVariant { get; set; }

        public string UserId { get; set; }

        public string TaskName { get; set; }

        public ReviewStatus Status { get; set; }

        public double Score { get; set; }

        public List<SubmissionViewModel> Submissions { get; set; }

        public AddSubmissionViewModel NewSubmissionViewModel { get; set; }
    }
}