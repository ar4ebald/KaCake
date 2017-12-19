using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Hosting;
using KaCake.Utils;
using KaCake.ControllersLogic;

namespace KaCake.Controllers
{
    public class TaskVariantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHostingEnvironment _env;
        private readonly TaskVariantLogic _taskVariantLogic;

        public TaskVariantController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

            _env = env;

            _taskVariantLogic = new TaskVariantLogic(context, userManager, roleManager);
        }

        [Authorize]
        public IActionResult View(int id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                TaskVariantViewModel viewModel = _taskVariantLogic.GetTaskVariant(userId, id);
                return View(viewModel);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{taskVariantId}")]
        public IActionResult GetTaskVariant(int taskVariantId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.GetTaskVariant(userId, taskVariantId));
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

        [HttpGet]
        public IActionResult Create(int id, int? taskVariantId)
        {
            string userId = _userManager.GetUserId(User);
            TaskGroup taskGroup = _context.TaskGroups.Find(id);
            if (!KaCakeUtils.IsCourseTeacher(_context, taskGroup.CourseId, userId))
            {
                return Challenge();
            }

            TaskVariant editingTaskVariant = null;
            if (taskVariantId.HasValue && (editingTaskVariant = _context.TaskVariants.Find(taskVariantId.Value)) != null)
            {
                return View(new TaskVariantViewModel()
                {
                    TaskGroupId = id,
                    Id = taskVariantId.Value,
                    Name = editingTaskVariant.Name,
                    Description = editingTaskVariant.Description,
                    IsUserTeacher = KaCakeUtils.IsCourseTeacher(_context, editingTaskVariant.TaskGroup.CourseId, userId)
                });
            }
            return View(new TaskVariantViewModel
            {
                TaskGroupId = id,
                IsUserTeacher = KaCakeUtils.IsCourseTeacher(_context, _context.TaskGroups.Find(id).CourseId, userId)
            });
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/[action]")]
        public IActionResult CreateTaskVariant([FromBody] TaskVariantViewModel taskVariant)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.CreateTaskVariant(userId, taskVariant));
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
        [HttpPost]
        [Route("api/[controller]/[action]/{taskVariantId}")]
        public IActionResult EditTaskVariant(int taskVariantId, [FromBody] TaskVariantViewModel taskVariant)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.EditTaskVariant(userId, taskVariantId, taskVariant));
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

        [HttpPost]
        public async Task<IActionResult> Create(TaskVariantViewModel taskVariant)
        {
            string userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                var testerFile = new FileInfo(Path.Combine(_env.WebRootPath, "App_Data", "Testers", taskVariant.Id + ".zip"));

                if (testerFile.Exists)
                    testerFile.Delete();

                if (taskVariant.TesterArchive != null && taskVariant.TesterArchive.Length != 0)
                {
                    using (var fileStream = new FileStream(testerFile.FullName, FileMode.Create))
                        await taskVariant.TesterArchive.CopyToAsync(fileStream);
                }

                TaskVariant editingTaskVariant;
                if ((editingTaskVariant = _context.TaskVariants.Find(taskVariant.Id)) != null)
                {
                    try
                    {
                        taskVariant = _taskVariantLogic.EditTaskVariant(userId, taskVariant.Id, taskVariant);
                    }
                    catch (NotFoundException)
                    {
                        return NotFound();
                    }
                    catch (IllegalAccessException)
                    {
                        Challenge();
                    }
                }
                else
                {
                    try
                    {
                        taskVariant = _taskVariantLogic.CreateTaskVariant(userId, taskVariant);
                    }
                    catch (NotFoundException)
                    {
                        return NotFound();
                    }
                    catch (IllegalAccessException)
                    {
                        Challenge();
                    }
                }
                return RedirectToAction(nameof(View), new { id = taskVariant.Id });
            }

            return View(taskVariant);
        }

        private void PopulateAddAssignmentData(int courseId, int taskVariantId)
        {
            ViewData["Reviewers"] = _context.Users
                .Include(user => user.TeachingCourses)
                .Where(user => user.TeachingCourses.Any(c => c.CourseId == courseId)) // is course teacher
                .Select(user => new SelectListItem
                {
                    Text = user.FullName,
                    Value = user.Id
                }).ToList();

            ViewData["UsersToAdd"] = _taskVariantLogic.GetStudentsCouldBeAdded(courseId, taskVariantId)
                .Select(user => new SelectListItem()
                {
                    Text = user.FullName,
                    Value = user.UserId
                }).ToList();

            ViewData["UsersToRemove"] = _taskVariantLogic.GetStudentsCouldBeRemoved(courseId, taskVariantId)
                .Select(user => new SelectListItem()
                {
                    Text = user.FullName,
                    Value = user.UserId
                }).ToList();
        }

        [Authorize]
        [Route("api/[controller]/[action]/{courseId}")]
        public IActionResult GetStudentsCouldBeAdded(int courseId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.GetStudentsCouldBeAdded(courseId));
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
        [Route("api/[controller]/[action]/{courseId}/{taskVariantId}")]
        public IActionResult GetStudentsCouldBeAdded(int courseId, int taskVariantId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.GetStudentsCouldBeAdded(courseId, taskVariantId));
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
        [Route("api/[controller]/[action]/{courseId}/{taskVariantId}")]
        public IActionResult GetStudentsCouldBeRemoved(int courseId, int taskVariantId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskVariantLogic.GetStudentsCouldBeRemoved(courseId, taskVariantId));
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

        [HttpGet]
        public IActionResult AddAssignments(int id)
        {
            string userId = _userManager.GetUserId(User);

            var taskVariant = _context.TaskVariants
                .Include(tv => tv.TaskGroup)
                .Include(tv => tv.Assignments)
                .FirstOrDefault(tv => tv.Id == id);

            var taskVariantAssignments = _context.Assignments
                .Include(ass => ass.User)
                .Join(taskVariant.Assignments,
                    ass => ass.TaskVariantId,
                    tvAss => tvAss.TaskVariantId,
                    (ass, tvAss) => ass);

            if (taskVariant == null)
                return NotFound();
            if (!KaCakeUtils.IsCourseTeacher(_context, taskVariant.TaskGroup.CourseId, userId))
            {
                return Challenge();
            }

            var editingVariant = new
            {
                taskVariant.Id,
                taskVariant.Name,
                UsersToRemove = taskVariantAssignments
                    .Select(assignment => new
                    {
                        assignment.UserId,
                        Name = assignment.User.FullName
                    }).ToList()
            };

            PopulateAddAssignmentData(taskVariant.TaskGroup.CourseId, taskVariant.Id);

            return View(new AddAsignmentsViewModel()
            {
                TaskVariantId = editingVariant.Id,
                TaskVariantName = editingVariant.Name,
                DeadlineUtc = DateTime.Now,
            });
        }

        [HttpPost]
        public IActionResult AddAssignments(AddAsignmentsViewModel viewModel)
        {
            string userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                if (_taskVariantLogic.AddAssignments(userId, viewModel.TaskVariantId, viewModel))
                {
                    return RedirectToAction("View", new { id = viewModel.TaskVariantId });
                }
            }

            var taskVariant = _context.TaskVariants
                .Include(tv => tv.TaskGroup)
                .FirstOrDefault(tv => tv.Id == viewModel.TaskVariantId);

            if (taskVariant == null)
            {
                return NotFound();
            }
            PopulateAddAssignmentData(taskVariant.TaskGroup.CourseId, taskVariant.Id);

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/[action]/{taskVariantId}")]
        public IActionResult AddAssignments(int taskVariantId, [FromBody] AddAsignmentsViewModel viewModel)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                if (_taskVariantLogic.AddAssignments(userId, taskVariantId, viewModel))
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
