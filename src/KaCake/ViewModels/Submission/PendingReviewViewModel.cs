using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;

namespace KaCake.ViewModels.Submission
{
    public class PendingReviewViewModel
    {
        public List<PendingReviewItem> Assignments { get; set; }

        public class PendingReviewItem
        {
            public int TaskVariantId { get; set; }
            public string UserId { get; set; }

            public string TaskVariantName { get; set; }
            public string UserName { get; set; }
        }
    }
}
