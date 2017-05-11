using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.TaskVariant;
using KaCake.Utils;
using Microsoft.AspNetCore.Identity;

namespace KaCake.ControllersLogic
{
    public class TaskGroupLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskGroupLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public TaskGroupViewModel GetTaskGroup(string userId, int taskGroupId)
        {
            TaskGroup taskGroup = _context.TaskGroups
                .Include(tg => tg.Course)
                .Include(tg => tg.Variants)
                .FirstOrDefault(tg => tg.Id == taskGroupId);

            if(taskGroup == null)
            {
                throw new NotFoundException();
            }

            return new TaskGroupViewModel
            {
                Id = taskGroupId,
                CourseId = taskGroup.CourseId,
                CourseName = taskGroup.Course.Name,
                Name = taskGroup.Name,
                Description = taskGroup.Description,
                IsCourseTeacher = KaCakeUtils.IsCourseTeacher(_context, taskGroup.CourseId, userId),
                Variants = taskGroup.Variants.Select(variant => new TaskVariantViewModel()
                {
                    Id = variant.Id,
                    Name = variant.Name,
                    Description = variant.Description
                }).ToList()
            };
        }

        public TaskGroupViewModel CreateTaskGroup(string userId, TaskGroupViewModel taskGroup)
        {
            if (!KaCakeUtils.IsCourseTeacher(_context, taskGroup.CourseId, userId))
            {
                throw new IllegalAccessException();
            }

            var entry = _context.TaskGroups.Add(new TaskGroup()
            {
                CourseId = taskGroup.CourseId,
                Name = taskGroup.Name,
                Description = taskGroup.Description
            });
            _context.SaveChanges();
            taskGroup.Id = entry.Entity.Id;

            return GetTaskGroup(userId, entry.Entity.Id);
        }

        public TaskGroupViewModel EditTaskGroup(string userId, int taskGroupId, TaskGroupViewModel taskGroup)
        {
            TaskGroup editingGroup = _context.TaskGroups.Find(taskGroupId);

            if(editingGroup == null)
            {
                throw new NotFoundException();
            }
            if(!KaCakeUtils.IsCourseTeacher(_context, editingGroup.CourseId, userId))
            {
                throw new IllegalAccessException();
            }

            editingGroup.Name = taskGroup.Name;
            editingGroup.Description = taskGroup.Description;
            _context.SaveChanges();

            return GetTaskGroup(userId, editingGroup.Id);
        }

    }
}
