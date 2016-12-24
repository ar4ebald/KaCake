using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.Submission;

namespace KaCake.ViewModels.Assignment
{
    public class ReviewViewModel
    {
        public string TaskGroupName { get; set; }
        public string TaskVariantName { get; set; }

        public string UserName { get; set; }

        public List<SubmissionViewModel> Submissions { get; set; }
    }
}
