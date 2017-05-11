using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.TaskGroup;
using KaCake.ViewModels.TaskVariant;
using KaCake.Utils;
using KaCake.ViewModels.UserInfo;

namespace KaCake.ControllersLogic
{
    public class TaskVariantLogic 
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TaskVariantLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public TaskVariantViewModel GetTaskVariant(string userId, int taskVariantId)
        {
            TaskVariant taskVariant = _context.TaskVariants
                .Include(tv => tv.TaskGroup)
                .Include(tv => tv.TaskGroup.Course)
                .Include(tv => tv.Assignments)
                .FirstOrDefault(tv => tv.Id == taskVariantId);

            if (taskVariant == null)
            {
                throw new NotFoundException();
            }

            return new TaskVariantViewModel()
            {
                Id = taskVariantId,
                Name = taskVariant.Name,
                Description = taskVariant.Description,
                TaskGroupId = taskVariant.TaskGroupId,
                TaskGroupName = taskVariant.TaskGroup.Name,
                CourseId = taskVariant.TaskGroup.CourseId,
                CourseName = taskVariant.TaskGroup.Course.Name,
                IsAssigned = taskVariant.Assignments.Any(assignment => assignment.UserId == userId),
                IsNeedingReview = taskVariant.Assignments.Any(assignment => assignment.ReviewerId == userId),
                AssignmentsCount = taskVariant.Assignments.Count,
                Assignments = taskVariant.Assignments
                    .Select(assignment => new AssignmentViewModel
                    {
                        TaskVariantId = taskVariant.Id,
                        Status = assignment.Status,
                        Score = assignment.Score,
                        TaskName = taskVariant.Name,
                        UserId = assignment.UserId
                    }).ToList(),
                IsUserTeacher = KaCakeUtils.IsCourseTeacher(_context, taskVariant.TaskGroup.CourseId, userId)
            };
        }

        public TaskVariantViewModel CreateTaskVariant(string userId, TaskVariantViewModel taskVariant)
        {
            if (!KaCakeUtils.IsCourseTeacher(_context, _context.TaskGroups.Find(taskVariant.TaskGroupId).CourseId, userId))
            {
                throw new IllegalAccessException();
            }

            var entity = _context.TaskVariants.Add(new TaskVariant()
            {
                TaskGroupId = taskVariant.TaskGroupId,
                Name = taskVariant.Name,
                Description = taskVariant.Description
            });
            _context.SaveChanges();

            return GetTaskVariant(userId, entity.Entity.Id);
        }

        public TaskVariantViewModel EditTaskVariant(string userId, int taskVariantId, TaskVariantViewModel taskVariant)
        {
            var editingTaskVariant = _context.TaskVariants
                .Include(tv => tv.TaskGroup)
                .FirstOrDefault(tv => tv.Id == taskVariantId);

            if (editingTaskVariant == null)
            {
                throw new NotFoundException();
            }
            if (!KaCakeUtils.IsCourseTeacher(_context, editingTaskVariant.TaskGroup.CourseId, userId))
            {
                throw new IllegalAccessException();
            }

            editingTaskVariant.Name = taskVariant.Name;
            editingTaskVariant.Description = taskVariant.Description;
            _context.SaveChanges();

            return GetTaskVariant(userId, editingTaskVariant.Id);
        }

        public IList<UserInfoViewModel> GetStudentsCouldBeAdded(int courseId)
        {
            Course course = _context.Courses
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                throw new NotFoundException();
            }

            return course.Students.Select(
                student => new UserInfoViewModel
                {
                    UserId = student.UserId,
                    UserName = _context.Users.Find(student.UserId)?.UserName,
                    FullName = _context.Users.Find(student.UserId)?.FullName
                }).ToList();
        }

        public IList<UserInfoViewModel> GetStudentsCouldBeAdded(int courseId, int taskVariantId)
        {
            Course course = _context.Courses
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                throw new NotFoundException();
            }

            var assignmentsForTaskVariant = _context.Assignments
                .Where(ass => ass.TaskVariantId == taskVariantId);

            return course.Students
                .Where(s => !assignmentsForTaskVariant.Any(ass => ass.UserId == s.UserId))
                .Select(
                student => new UserInfoViewModel
                {
                    UserId = student.UserId,
                    UserName = _context.Users.Find(student.UserId)?.UserName,
                    FullName = _context.Users.Find(student.UserId)?.FullName
                }).ToList();
        }

        public IList<UserInfoViewModel> GetStudentsCouldBeRemoved(int courseId, int taskVariantId)
        {
            Course course = _context.Courses
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                throw new NotFoundException();
            }

            var assignmentsForTaskVariant = _context.Assignments
                .Where(ass => ass.TaskVariantId == taskVariantId);

            return course.Students
                .Where(s => assignmentsForTaskVariant.Any(ass => ass.UserId == s.UserId))
                .Select(
                student => new UserInfoViewModel
                {
                    UserId = student.UserId,
                    UserName = _context.Users.Find(student.UserId)?.UserName,
                    FullName = _context.Users.Find(student.UserId)?.FullName
                }).ToList();
        }
        
        public bool AddAssignments(string userId, int taskVariantId, AddAsignmentsViewModel viewModel)
        {
            var taskVariant = _context.TaskVariants
                .Include(tv => tv.TaskGroup)
                .FirstOrDefault(tv => tv.Id == taskVariantId);

            if (taskVariant == null)
            {
                throw new NotFoundException();
            }
            if (!KaCakeUtils.IsCourseTeacher(_context, taskVariant.TaskGroup.CourseId, userId))
            {
                throw new IllegalAccessException();
            }
            if (viewModel.UsersToRemove != null)
            {
                _context.Assignments.RemoveRange(
                    _context.Assignments.Where(
                        assignment => viewModel.UsersToRemove.Contains(assignment.UserId)
                    )
                );
            }

            if (viewModel.UsersToAdd != null)
            {
                _context.Assignments.AddRange(viewModel.UsersToAdd.Select(user => new Assignment()
                {
                    TaskVariantId = viewModel.TaskVariantId,
                    DeadlineUtc = viewModel.DeadlineUtc,
                    UserId = user,
                    ReviewerId = viewModel.ReviewerId
                }));
            }

            _context.SaveChanges();

            return true;

        }
    }
}
