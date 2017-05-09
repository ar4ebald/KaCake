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
using KaCake.Utils;
using KaCake.ViewModels.UserInfo;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            return View(new ViewModels.Course.IndexViewModel()
            {
                Courses = _context.Courses
                    .Select(course => new CourseViewModel()
                    {
                        Id = course.Id,
                        Name = course.Name
                    })
                    .ToList()
            });
        }

        [HttpGet]
        public IActionResult View(int id)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            var viewingCourse = _context.Courses
                .Include(course => course.TaskGroups)
                .Include(course => course.Teachers)
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
                Teachers = viewingCourse.Teachers.Select(
                    teacher => KaCakeUtils.createUserInfoViewModel(_context, teacher.TeacherId))
                    .ToList(),
                IsUserATeacher = viewingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId)
            });
        }

        [HttpGet]
        public IActionResult Create(int? id)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            var teachersToAdd = _context.Users.Select(user =>
            new SelectListItem
            {
                Text = user.FullName,
                Value = user.Id
            }).ToList();

            Course editingCourse;
            if (id.HasValue 
                && (editingCourse = 
                    _context.Courses.Include(c => c.Teachers)
                        .FirstOrDefault(c => c.Id == id.Value)) != null)
            {
                // Only teachers could edit course
                if (editingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId))
                {
                    teachersToAdd.RemoveAll(t => editingCourse.Teachers.Any(teacher => teacher.TeacherId.Equals(t.Value)));

                    ViewData["TeachersToAdd"] = teachersToAdd;
                    ViewData["TeachersToRemove"] = editingCourse.Teachers
                        .Where(teacher => KaCakeUtils.isAppointer(editingCourse, callerId, teacher))
                        .Select(teacher => new SelectListItem
                        {
                            Text = _context.Users.Find(teacher.TeacherId).FullName,
                            Value = teacher.TeacherId
                        });

                    return View(new CreateViewModel()
                    {
                        Id = id.GetValueOrDefault(),
                        Name = editingCourse.Name,
                        Description = editingCourse.Description
                    });
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                ViewData["TeachersToAdd"] = teachersToAdd;
                ViewData["TeachersToRemove"] = new List<SelectListItem>();
                return View();
            }
        }

        [HttpPost]
        public IActionResult Create(CreateViewModel course)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                Course editingCourse;
                if (course.Id.HasValue && (editingCourse =
                    _context.Courses.Include(c => c.Teachers)
                        .FirstOrDefault(c => c.Id == course.Id.Value)) != null)
                {
                    // The course could be edited only by a teacher of that course
                    if (editingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId))
                    {
                        editingCourse.Name = course.Name;
                        editingCourse.Description = course.Description;

                        if (course.TeachersToAdd != null)
                        {
                            course.TeachersToAdd.Select(
                                teacherId => new CourseTeacher2
                                {
                                    CourseId = course.Id.GetValueOrDefault(),
                                    TeacherId = teacherId,
                                    AppointerId = callerId

                                }
                            ).ToList().ForEach(teacher => editingCourse.Teachers.Add(teacher));
                        }

                        if (course.TeachersToRemove != null)
                        {
                            var teachersToRemove = course.TeachersToRemove
                                .Where(userId => editingCourse.Teachers.Any(teacher => teacher.TeacherId.Equals(userId)))
                                .Select(teacherId => editingCourse.Teachers.First(teacher => teacher.TeacherId.Equals(teacherId)));

                            foreach (var teacherToRemove in teachersToRemove)
                            {
                                // Only appointer can remove teachers appointed by him
                                if (KaCakeUtils.isAppointer(editingCourse, callerId, teacherToRemove))
                                {
                                    editingCourse.Teachers.Remove(teacherToRemove);
                                }
                                else
                                {
                                    // Maybe there should be an error
                                }
                            }
                        }
                    }
                }
                else
                {
                    Course courseToAdd = new Course()
                    {
                        Name = course.Name,
                        Description = course.Description
                    };
                    _context.Courses.Add(courseToAdd);
                    _context.SaveChanges();

                    // And now the id of the course could be obtained
                    CourseTeacher2 courseTeacher = new CourseTeacher2
                    {
                        CourseId = courseToAdd.Id,
                        TeacherId = callerId,
                        AppointerId = callerId,
                    };
                    var teachers = new List<CourseTeacher2>();
                    teachers.Add(courseTeacher);

                    // Add all the 'Teachers to add'
                    foreach(string teacherToAddId in course.TeachersToAdd)
                    {
                        teachers.Add(new CourseTeacher2
                        {
                            CourseId = courseToAdd.Id,
                            TeacherId = teacherToAddId,
                            AppointerId = callerId
                        });
                    }

                    // Then set the 'teacher' field
                    courseToAdd.Teachers = teachers;
                }
                _context.SaveChanges();
                return RedirectToAction("Index", "Course");
            }
            return View(course);
        }
    }
}
