﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KaCake.ViewModels.UserInfo;
using KaCake.Data;
using KaCake.Data.Models;

namespace KaCake.Utils
{
    public class KaCakeUtils
    {
        public static UserInfoViewModel createUserInfoViewModel(ApplicationDbContext context, string userId)
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

        public static bool IsCourseTeacher(ApplicationDbContext context, int courseId, string userId)
        {
            Course course = context.Courses
                .Include(c => c.Teachers)
                .FirstOrDefault(c => c.Id == courseId);

            if (course != null)
            {
                return course.Teachers.Any(teacher => teacher.TeacherId.Equals(userId));
            }

            return false;
        }

        public static bool isAppointer(Course course, string possibleAppointerId, CourseTeacher teacher)
        {
            if(teacher.Appointer.TeacherId.Equals(possibleAppointerId))
            {
                return true;
            }
            if(teacher.Appointer.TeacherId.Equals(teacher.TeacherId))
            {
                return false; // Stop here
            }

            // Find a CourseTeacher with id equals to teacher.AppointerId
            CourseTeacher appointer = course.Teachers.FirstOrDefault(a => teacher.Appointer.TeacherId.Equals(a.TeacherId));
            
            if(appointer != null)
            {
                return isAppointer(course, possibleAppointerId, appointer);
            }

            return false;
        }
    }
}
