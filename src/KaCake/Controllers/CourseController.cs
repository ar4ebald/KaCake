using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.Course;
using IndexViewModel = KaCake.ViewModels.Course.IndexViewModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new ViewModels.Course.IndexViewModel()
            {
                Courses = _context.Courses.Select(course => new CourseViewModel()
                {
                    Id = course.Id,
                    Name = course.Name,
                }).ToList()
            });
        }

        [HttpGet]
        public IActionResult View(int id)
        {
            var viewingCourse = _context.Courses
                .Include(course => course.TaskGroups)
                .FirstOrDefault(course => course.Id == id);

            if (viewingCourse == null)
                return NotFound();

            return View(new CourseViewModel()
            {
                Id = viewingCourse.Id,
                Name = viewingCourse.Name,
                Description = viewingCourse.Description,
                TaskGroups = viewingCourse.TaskGroups.Select(taskGroup => new TaskGroupViewModel()
                {
                    Id = taskGroup.Id,
                    Name = taskGroup.Name
                }).ToList()
            });
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(int? id)
        {
            Course editingCourse;
            if (id.HasValue && (editingCourse = _context.Courses.Find(id.Value)) != null)
            {
                return View(new CreateViewModel()
                {
                    Id = id.GetValueOrDefault(),
                    Name = editingCourse.Name,
                    Description = editingCourse.Description
                });
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(CreateViewModel course)
        {
            if (ModelState.IsValid)
            {
                Course editingCourse;
                if ((editingCourse = _context.Courses.Find(course.Id)) != null)
                {
                    editingCourse.Name = course.Name;
                    editingCourse.Description = course.Description;
                }
                else
                {
                    _context.Courses.Add(new Course()
                    {
                        Name = course.Name,
                        Description = course.Description
                    });
                }
                _context.SaveChanges();
                return RedirectToAction("Index", "Course");
            }
            return View(course);
        }
    }
}
