using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data.Models;

using Models = KaCake.Data.Models;

namespace KaCake.ViewModels.Course
{
    public class IndexViewModel
    {
        public ICollection<CourseViewModel> Courses { get; set; }
    }
}
