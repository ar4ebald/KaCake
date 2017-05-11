using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.UserInfo;
using System.ComponentModel.DataAnnotations;

namespace KaCake.ViewModels.Course
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IList<TaskGroupViewModel> TaskGroups { get; set; }

        public IList<UserInfoViewModel> Students { get; set; }

        public IList<UserInfoViewModel> Teachers { get; set; }

        public bool IsUserATeacher { get; set; }

        public bool CanDeleteThisCourse { get; set; }
    }
}
