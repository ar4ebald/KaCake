using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KaCake.Data.Models
{
    // Don't ask what ColumnAttribute does
    public class Assignment
    {
        [ForeignKey(nameof(TaskVariant))]
        [Column(Order = 0)]
        public int TaskVariantId { get; set; }
        public TaskVariant TaskVariant { get; set; }

        [ForeignKey(nameof(User))]
        [Column(Order = 1)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<Submission> Submissions { get; set; }

        public bool IsChecked { get; set; }
        public double Score { get; set; }
    }
}
