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

namespace KaCake.Controllers
{
    public class TaskGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskGroupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int id, int? taskGroupId)
        {
            bool isCourseTeacher = KaCakeUtils.IsCourseTeacher(_context, id, _userManager.GetUserId(HttpContext.User));
            if(!isCourseTeacher)
            {
                return Challenge();
            }

            TaskGroup editingGroup;
            if (taskGroupId.HasValue && (editingGroup = _context.TaskGroups.Find(taskGroupId.Value)) != null)
            {
                return View(new TaskGroupViewModel()
                {
                    CourseId = id,
                    Id = taskGroupId.Value,
                    Name = editingGroup.Name,
                    Description = editingGroup.Description,
                    IsCourseTeacher = isCourseTeacher
                });
            }

            return View(new TaskGroupViewModel()
            {
                CourseId = id,
                IsCourseTeacher = isCourseTeacher
            });
        }

        [HttpPost]
        public IActionResult Create(TaskGroupViewModel taskGroup)
        {
            if (ModelState.IsValid)
            {
                TaskGroup editedTaskGroup;
                if ((editedTaskGroup = _context.TaskGroups.Find(taskGroup.Id)) != null)
                {
                    if (!KaCakeUtils.IsCourseTeacher(_context, editedTaskGroup.CourseId, _userManager.GetUserId(HttpContext.User)))
                    {
                        return Challenge();
                    }

                    editedTaskGroup.Name = taskGroup.Name;
                    editedTaskGroup.Description = taskGroup.Description;
                    _context.SaveChanges();
                }
                else
                {
                    if (!KaCakeUtils.IsCourseTeacher(_context, taskGroup.CourseId, _userManager.GetUserId(HttpContext.User)))
                    {
                        return Challenge();
                    }

                    var entry = _context.TaskGroups.Add(new TaskGroup()
                    {
                        CourseId = taskGroup.CourseId,
                        Name = taskGroup.Name,
                        Description = taskGroup.Description
                    });
                    _context.SaveChanges();
                    taskGroup.Id = entry.Entity.Id;
                }

                return RedirectToAction("View", new { id = taskGroup.Id });
            }

            return View(taskGroup);
        }

        [Authorize]
        public IActionResult View(int id)
        {
            var viewModel = _context.TaskGroups
                .Where(taskGroup => taskGroup.Id == id)
                .Select(taskGroup => new TaskGroupViewModel()
                {
                    Id = taskGroup.Id,
                    CourseId = taskGroup.CourseId,
                    CourseName = taskGroup.Course.Name,
                    Name = taskGroup.Name,
                    Description = taskGroup.Description,
                    Variants = taskGroup.Variants.Select(variant => new TaskVariantViewModel()
                    {
                        Id = variant.Id,
                        Name = variant.Name,
                        Description = variant.Description
                    }).ToList(),
                    IsCourseTeacher = KaCakeUtils.IsCourseTeacher(_context, taskGroup.CourseId, _userManager.GetUserId(HttpContext.User))
                })
                .FirstOrDefault();

            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }
    }
}
