namespace KaCake.Data.Models
{
    public class CourseEnrollment
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
