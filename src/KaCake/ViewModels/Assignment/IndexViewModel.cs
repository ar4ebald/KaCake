using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;

namespace KaCake.ViewModels.Assignment
{
    public class IndexViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public int TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }

        public int TaskVariantId { get; set; }
        public string TaskVariantName { get; set; }
        public List<AssignmentViewModel> Assignments { get; set; }

        public bool UserIsTeacher { get; set; }
    }
}
