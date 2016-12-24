using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.TaskGroup;
using KaCake.ViewModels.TaskVariant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KaCake.Controllers
{
    public class TaskGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(int id)
        {
            return View(new TaskGroupViewModel()
            {
                CourseId = id
            });
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(TaskGroupViewModel taskGroup)
        {
            if (ModelState.IsValid)
            {
                EntityEntry<TaskGroup> addedEntity = _context.TaskGroups.Add(new TaskGroup()
                {
                    CourseId = taskGroup.CourseId,
                    Name = taskGroup.Name,
                    Description = taskGroup.Description
                });
                _context.SaveChanges();

                return RedirectToAction("View", new { id = addedEntity.Entity.Id });
            }

            return View(taskGroup);
        }

        [Authorize]
        public IActionResult View(int id)
        {
            var viewingTaskGroup = _context.TaskGroups
                .Include(taskGroup => taskGroup.Variants)
                .FirstOrDefault(taskGroup => taskGroup.Id == id);

            if (viewingTaskGroup == null)
                return NotFound();

            return View(new TaskGroupViewModel()
            {
                Id = viewingTaskGroup.Id,
                Name = viewingTaskGroup.Name,
                Description = viewingTaskGroup.Description,
                Variants = viewingTaskGroup.Variants.Select(variant => new TaskVariantViewModel()
                {
                    Id = variant.Id,
                    Name = variant.Name,
                    Description = variant.Description
                }).ToList()
            });
        }
    }
}
