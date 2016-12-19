using System.ComponentModel.DataAnnotations;

namespace KaCake.Data.Models
{
    public class Course
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
    }
}
