using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KaCake.Data.Models
{
    public enum TestingStatus
    {
        Pending,
        Testing,

        Failed,
        Passed
    }

    public class Submission
    {
        public int Id { get; set; }

        public Assignment Assignment { get; set; }

        public DateTime Time { get; set; }

        public string Path { get; set; }


        public TestingStatus Status { get; set; }

        public DateTime PickedForTestingTimeUtc { get; set; }

        public string ReviewTitle { get; set; }
        public string ReviewMessage { get; set; }
    }
}
