using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.Submission;
using KaCake.ViewModels.TaskGroup;
using KaCake.ViewModels.TaskVariant;
using SharpCompress.Readers;
using IndexViewModel = KaCake.ViewModels.Assignment.IndexViewModel;
using KaCake.Utils;
using KaCake.ControllersLogic;

namespace KaCake.Controllers
{
    public class AssignmentController : Controller
    {
        private static readonly HashSet<string> ArchiveExtensions = new HashSet<string>(new[]
        {
            ".zip", ".rar", ".tar", ".7z"
        });

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _env;

        private readonly AssignmentLogic _assignmentLogic;

        public AssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;

            _assignmentLogic = new AssignmentLogic(context, userManager, env);
        }
        
        [Authorize]
        public IActionResult Index(int id)
        {
            var taskData = _context.TaskVariants
                .Where(taskVariant => taskVariant.Id == id)
                .Select(taskVariant => new
                {
                    CourseId = taskVariant.TaskGroup.CourseId,
                    CourseName = taskVariant.TaskGroup.Course.Name,
                    TaskGroupId = taskVariant.TaskGroupId,
                    TaskGroupName = taskVariant.TaskGroup.Name,
                    TaskVariantId = taskVariant.Id,
                    TaskVariantName = taskVariant.Name
                }).FirstOrDefault();

            try
            {
                var assignments = _assignmentLogic.GetAssignmentsForTaskVariant(id);

                return View(new IndexViewModel()
                {
                    CourseId = taskData.CourseId,
                    CourseName = taskData.CourseName,
                    TaskGroupId = taskData.TaskGroupId,
                    TaskGroupName = taskData.TaskGroupName,
                    TaskVariantId = taskData.TaskVariantId,
                    TaskVariantName = taskData.TaskVariantName,
                    Assignments = assignments,
                    IsCourseTeacher = KaCakeUtils.IsCourseTeacher(_context, taskData.CourseId, _userManager.GetUserId(HttpContext.User))
                });
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{taskVariantId}")]
        public IActionResult GetAssignmentsForTaskVariant(int taskVariantId)
        {
            try
            {
                return new ObjectResult(_assignmentLogic.GetAssignmentsForTaskVariant(taskVariantId));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        // View
        [Authorize]
        public IActionResult View(int id)
        {
            var userId = _userManager.GetUserId(User);
            try
            {
                AssignmentViewModel viewingAssignment = _assignmentLogic.GetAssignment(userId, id);
                return View(viewingAssignment);
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        // api
        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetAssignment(int taskVariantId)
        {
            try
            {
                return new ObjectResult(_assignmentLogic.GetAssignment(_userManager.GetUserId(User), taskVariantId));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddSubmission_view(AssignmentViewModel viewModel)
        {
            try
            {
                viewModel = _assignmentLogic.AddSubmission(ModelState, _userManager.GetUserId(User), viewModel);
                return RedirectToAction("View", new { id = viewModel.NewSubmissionViewModel.TaskVariantId });
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
        [Route("api/[controller]/[action]")]
        public IActionResult AddSubmission([FromBody] AssignmentViewModel viewModel)
        {
            try
            {
                viewModel = _assignmentLogic.AddSubmission(ModelState, _userManager.GetUserId(User), viewModel);
                return Ok();
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
        public IActionResult Personal()
        {
            var userId = _userManager.GetUserId(User);
            return View(_assignmentLogic.Personal(userId));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetPersonalAssignments()
        {
            return new ObjectResult(_assignmentLogic.Personal(_userManager.GetUserId(User)));
        }
        
        [Authorize]
        public IActionResult PendingReview()
        {
            string userId = _userManager.GetUserId(User);
            return View(_assignmentLogic.PendingReview(userId));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetPersonalPendingReview()
        {
            return new ObjectResult(_assignmentLogic.PendingReview(_userManager.GetUserId(User)));
        }

        [Authorize]
        [HttpGet]
        [Route("[controller]/[action]/{variantId}/{userId}")]
        public IActionResult Review(int variantId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            try
            {
                var viewModel = _assignmentLogic.Review(currentUserId, variantId, userId);
                return View(viewModel);
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
        [HttpGet]
        [Route("api/[controller]/[action]/{variantId}/{userId}")]
        public IActionResult ReviewAssignment(int variantId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            try
            {
                var viewModel = _assignmentLogic.Review(currentUserId, variantId, userId);
                return new ObjectResult(viewModel);
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
        [Route("[controller]/[action]/{variantId}/{userId}")]
        public IActionResult Review(int variantId, string userId, double Score)
        {
            var currentUserId = _userManager.GetUserId(User);

            try
            {
                _assignmentLogic.Review(currentUserId, variantId, userId, Score);
                return RedirectToAction("Review", "Assignment", new { variantId, userId });
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
        [Route("api/[controller]/[action]/{variantId}/{userId}/{score}")]
        public IActionResult ReviewAssignment(int variantId, string userId, double score)
        {
            var currentUserId = _userManager.GetUserId(User);

            try
            {
                var viewModel = _assignmentLogic.Review(currentUserId, variantId, userId, score);
                return new ObjectResult(viewModel);
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
        public IActionResult Export(int id)
        {
            try
            {
                var result = _assignmentLogic.Export(id);
                return File(result, "application/csv", "report.csv");
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{id}")]
        public IActionResult ExportToExcel(int id)
        {
            try
            {
                var result = _assignmentLogic.Export(id);
                return File(result, "application/csv", "report.csv");
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
