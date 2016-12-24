using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.TaskGroup;

namespace KaCake.ViewModels.Course
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IList<TaskGroupViewModel> TaskGroups { get; set; }
    }
}
