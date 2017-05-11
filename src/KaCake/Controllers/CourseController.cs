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
                var courseVm = _courseLogic.GetCourse(callerId, id);
                return View(courseVm);
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

        [Route("api/[controller]/[action]")]
        public IActionResult GetSudentsCouldBeAdded()
        {
            try
            {
                return new ObjectResult(_courseLogic.GetStudentsCouldBeAdded());
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetSudentsCouldBeAdded(int courseId)
        {
            try
            {
                return new ObjectResult(_courseLogic.GetStudentsCouldBeAdded(courseId));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetSudentsCouldBeRemoved(int courseId)
        {
            try
            {
                return new ObjectResult(_courseLogic.GetStudentsCouldBeRemoved(courseId));
            }
            catch (NotFoundException)
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
                        ViewData["StudentsToAdd"] = _courseLogic.GetStudentsCouldBeAdded(editingCourse.Id)
                            .Select(student => new SelectListItem
                            {
                                Text = student.FullName,
                                Value = student.UserId
                            });
                        ViewData["StudentsToRemove"] = _courseLogic.GetStudentsCouldBeRemoved(editingCourse.Id)
                            .Select(student => new SelectListItem
                            {
                                Text = student.FullName,
                                Value = student.UserId
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

                ViewData["StudentsToAdd"] = _courseLogic.GetStudentsCouldBeAdded()
                    .Select(student => new SelectListItem
                    {
                        Text = student.FullName,
                        Value = student.UserId
                    });
                ViewData["StudentsToRemove"] = new List<SelectListItem>();
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        [Route("[controller]/[action]")]
        public IActionResult Create(CreateViewModel course)
        {
            string callerId = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                CourseViewModel result;

                Course editingCourse;

                try
                {
                    if (course.Id.HasValue && (editingCourse =
                        _context.Courses.Include(c => c.Teachers)
                            .FirstOrDefault(c => c.Id == course.Id.Value)) != null)
                    {
                        result = _courseLogic.Edit(callerId, editingCourse.Id, course);
                    }
                    else
                    {
                        result = _courseLogic.Create(callerId, course);
                    }
                    return RedirectToAction("Index", "Course");
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
                return new ObjectResult(_courseLogic.Create(callerId, course));
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
                return new ObjectResult(_courseLogic.Edit(callerId, courseId, course));
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

        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult CanDeleteCourse(int courseId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                return new ObjectResult(_courseLogic.CanDeleteCourse(userId, courseId));
            }
            catch (IllegalAccessException)
            {
                return Challenge();
            }
        }

        [Authorize]
        [Route("[controller]/[action]/{courseId}")]
        public IActionResult Delete(int courseId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                if (_courseLogic.Delete(userId, courseId))
                {
                    return RedirectToAction(nameof(Index), "Course");
                }
                else
                {
                    return StatusCode(500);
                }
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

        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult DeleteCourse(int courseId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                if (_courseLogic.Delete(userId, courseId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
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
