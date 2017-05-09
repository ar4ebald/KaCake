using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.UserInfo;
using KaCake.Data;
using KaCake.Data.Models;

namespace KaCake.Utils
{
    public class KaCakeUtils
    {
        public static  UserInfoViewModel createUserInfoViewModel(ApplicationDbContext context, string userId)
        {
            return mapToUserInfoVMs(context, userId).FirstOrDefault();
        }

        public static IList<UserInfoViewModel> mapToUserInfoVMs(ApplicationDbContext context, params string[] usersIds)
        {
            return context.Users
                .Join(usersIds, user => user.Id, userId => userId, 
                (user, userId) => new UserInfoViewModel
                {
                    UserId = userId,
                    UserName = user.UserName,
                    FullName = user.FullName
                })
                .ToList();
        }

        public static bool isCourseTeacher(ApplicationDbContext context, string courseId, string userId)
        {
            Course course = context.Courses.Find(courseId);

            if(course != null)
            {
                return course.Teachers.Any(teacher => teacher.TeacherId.Equals(userId));
            }

            return false;
        }
    }
}
