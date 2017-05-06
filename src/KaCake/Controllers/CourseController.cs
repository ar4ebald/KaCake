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
using Microsoft.AspNetCore.Identity;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class CourseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
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
                }).ToList(),
                UserIsTeacher = viewingCourse
                    .Teachers.Any(teacher => teacher.Id == _userManager.GetUserId(User))
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create(int? id)
        {
            Course editingCourse;
            if (id.HasValue && (editingCourse = _context.Courses.Find(id.Value)) != null)
            {
                var userId = _userManager.GetUserId(User);
                if (!editingCourse.Teachers.Any(t => t.Id.Equals(userId)))
                {
                    return View();
                }

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
        [Authorize]
        public IActionResult Create(CreateViewModel course)
        {
            if (ModelState.IsValid)
            {
                Course editingCourse;
                if (course.Id.HasValue && (editingCourse = _context.Courses.Find(course.Id)) != null)
                {
                    var userId = _userManager.GetUserId(User);
                    if(!editingCourse.Teachers.Any(t => t.Id.Equals(userId)))
                    {
                        return View(course);
                    }

                    editingCourse.Name = course.Name;
                    editingCourse.Description = course.Description;
                    var toRemove = course?.UsersToRemove.SelectMany(userToAdd =>
                        _context.Users.Where(user => user.Id.Equals(userToAdd))
                    );
                    foreach (var rm in toRemove)
                    {
                        editingCourse.Teachers.Remove(rm);
                    }
                    editingCourse.Teachers.Concat(course?.UsersToAdd.SelectMany(userToAdd =>
                            _context.Users.Where(user => user.Id.Equals(userToAdd))
                    ).ToList());
                }
                else
                {
                    _context.Courses.Add(new Course()
                    {
                        Name = course.Name,
                        Description = course.Description,
                        Teachers = course?.UsersToAdd.SelectMany(userToAdd =>
                            _context.Users.Where(user => user.Id.Equals(userToAdd))
                        ).ToList()
                    });
                }
                _context.SaveChanges();
                return RedirectToAction("Index", "Course");
            }
            return View(course);
        }
    }
}
