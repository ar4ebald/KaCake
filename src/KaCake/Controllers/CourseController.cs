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
using KaCake.ControllersLogic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly CourseLogic _courseLogic;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            _courseLogic = new CourseLogic(context, userManager);
        }

        public IActionResult Index()
        {
            return View(new ViewModels.Course.IndexViewModel()
            {
                Courses = _courseLogic.GetAllCourses()
            });
        }

        [Route("api/[controller]/[action]")]
        public IActionResult GetAllCourses()
        {
            return new ObjectResult(_courseLogic.GetAllCourses());
        }

        [HttpGet]
        [Authorize]
        public IActionResult View(int id)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            try
            {
                return View(_courseLogic.GetCourse(callerId, id));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }
        
        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetCourse(int courseId)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            try
            {
                return new ObjectResult(_courseLogic.GetCourse(callerId, courseId));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetTeachersCouldBeAdded()
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            return new ObjectResult(_courseLogic
                .GetTeachersCouldBeAdded(callerId));
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetTeachersCouldBeAdded(int courseId)
        {
            try
            {
                return new ObjectResult(_courseLogic
                    .GetTeachersCouldBeAdded(_userManager.GetUserId(HttpContext.User), courseId));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetTeachersCouldBeRemoved(int courseId)
        {
            try
            {
                return new ObjectResult(_courseLogic
                    .GetTeachersCouldBeRemoved(_userManager.GetUserId(HttpContext.User), courseId));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }


        [HttpGet]
        [Authorize]
        public IActionResult Create(int? id)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            Course editingCourse;
            if (id.HasValue 
                && (editingCourse = 
                    _context.Courses.Include(c => c.Teachers)
                        .FirstOrDefault(c => c.Id == id.Value)) != null)
            {
                // Only teachers could edit course
                if (editingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId))
                {
                    try
                    {
                        ViewData["TeachersToAdd"] = _courseLogic.GetTeachersCouldBeAdded(callerId, editingCourse.Id)
                            .Select(
                                t =>
                                new SelectListItem
                                {
                                    Text = t.FullName,
                                    Value = t.UserId
                                });

                        ViewData["TeachersToRemove"] = _courseLogic.GetTeachersCouldBeRemoved(callerId, editingCourse.Id)
                            .Select(teacher => new SelectListItem
                            {
                                Text = teacher.FullName,
                                Value = teacher.UserId
                            });
                    }
                    catch(NotFoundException)
                    {
                        return NotFound();
                    }

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
                ViewData["TeachersToAdd"] = _courseLogic.GetTeachersCouldBeAdded(callerId)
                        .Select(
                            t =>
                            new SelectListItem
                            {
                                Text = t.FullName,
                                Value = t.UserId
                            }); ;
                ViewData["TeachersToRemove"] = new List<SelectListItem>();
                return View();
            }
        }

        [HttpPost]
        [Authorize]
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


        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult CreateCourse([FromBody] CreateViewModel course)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            try
            {
                return new ObjectResult(_courseLogic.Create(callerId, ModelState, course));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
            catch(IllegalAccessException)
            {
                return Challenge();
            }
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult EditCourse(int courseId, [FromBody] CreateViewModel course)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            try
            {
                return new ObjectResult(_courseLogic.Edit(callerId, ModelState, courseId, course));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (IllegalAccessException)
            {
                return Challenge();
            }
        }
    }
}
