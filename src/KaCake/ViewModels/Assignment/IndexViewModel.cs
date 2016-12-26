using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;

namespace KaCake.ViewModels.Assignment
{
    public class IndexViewModel
    {
        public string TaskGroupName { get; set; }
        public string TaskVariantName { get; set; }
        public List<AssignmentViewModel> Assignments { get; set; }
    }
}
