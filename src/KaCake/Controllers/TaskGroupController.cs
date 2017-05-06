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
using Microsoft.AspNetCore.Identity;

namespace KaCake.Controllers
{
    public class TaskGroupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TaskGroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create(int id, int? taskGroupId)
        {
            TaskGroup editingGroup;
            if (taskGroupId.HasValue && (editingGroup = _context.TaskGroups.Find(taskGroupId.Value)) != null)
            {
                return View(new TaskGroupViewModel()
                {
                    CourseId = id,
                    Id = taskGroupId.Value,
                    Name = editingGroup.Name,
                    Description = editingGroup.Description,
                    UserIsTeacher = editingGroup.Course
                        .Teachers.Any(teacher => teacher.Id == _userManager.GetUserId(User))
                });
            }

            return View(new TaskGroupViewModel()
            {
                CourseId = id
            });
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(TaskGroupViewModel taskGroup)
        {
            if (ModelState.IsValid)
            {
                TaskGroup editedTaskGroup;
                if ((editedTaskGroup = _context.TaskGroups.Find(taskGroup.Id)) != null)
                {
                    editedTaskGroup.Name = taskGroup.Name;
                    editedTaskGroup.Description = taskGroup.Description;
                    _context.SaveChanges();
                }
                else
                {
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
                    }).ToList()
                })
                .FirstOrDefault();

            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }
    }
}
