using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KaCake.ViewModels.Course
{
    public class CreateViewModel
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
