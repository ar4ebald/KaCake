using System.ComponentModel.DataAnnotations.Schema;

namespace KaCake.Data.Models
{
    public class CourseEnrollment
    {
        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
