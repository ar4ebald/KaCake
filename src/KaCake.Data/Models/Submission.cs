using System;
using System.Collections.Generic;
using System.Text;

namespace KaCake.Data.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public Assignment Assignment { get; set; }

        public DateTime Time { get; set; }

        public string Path { get; set; }
    }
}
