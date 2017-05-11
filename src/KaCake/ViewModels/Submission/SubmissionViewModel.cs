using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data.Models;

namespace KaCake.ViewModels.Submission
{
    public class SubmissionViewModel
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public TestingStatus Status { get; set; }

        public string ReviewTitle { get; set; }
        public string ReviewMessage { get; set; }

        public override string ToString() => $"{Time} - {Status}";
    }
}
