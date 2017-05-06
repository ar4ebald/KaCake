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

        public AssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
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

            if (taskData == null)
                return NotFound();

            return View(new ViewModels.Assignment.IndexViewModel()
            {
                CourseId = taskData.CourseId,
                CourseName = taskData.CourseName,
                TaskGroupId = taskData.TaskGroupId,
                TaskGroupName = taskData.TaskGroupName,
                TaskVariantId = taskData.TaskVariantId,
                TaskVariantName = taskData.TaskVariantName,
                Assignments = _context.Assignments
                    .Where(assignment => assignment.TaskVariantId == id)
                    .Select(assignment => new AssignmentViewModel()
                    {
                        TaskVariantId = id,
                        UserId = assignment.UserId,
                        UserName = assignment.User.FullName,
                        Score = assignment.Score,
                        Status = assignment.Status
                    }).ToList(),
                UserIsTeacher = _context.Courses.FirstOrDefault(c => c.Id == taskData.CourseId)
                    .Teachers.Any(teacher => teacher.Id == _userManager.GetUserId(User))
            });
        }

        [Authorize]
        public IActionResult View(int id)
        {
            var userId = _userManager.GetUserId(User);
            AssignmentViewModel viewingAssignment = _context.Assignments
                .Include(assignment => assignment.TaskVariant)
                .Include(assignment => assignment.TaskVariant.Assignments)
                .Where(assignment => assignment.UserId == userId && assignment.TaskVariantId == id)
                .Select(assignment => new AssignmentViewModel()
                {
                    TaskVariant = new TaskVariantViewModel()
                    {
                        Name = assignment.TaskVariant.Name,
                        Description = assignment.TaskVariant.Description
                    },
                    Submissions = assignment.Submissions.Select(submission => new SubmissionViewModel()
                    {
                        Id = submission.Id,
                        Time = submission.Time
                    }).ToList(),
                    NewSubmissionViewModel = new AddSubmissionViewModel()
                    {
                        TaskVariantId = id
                    },
                    Score = assignment.Score,
                    Status = assignment.Status
                }).FirstOrDefault();

            if (viewingAssignment == null)
                return NotFound();

            return View(viewingAssignment);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddSubmission(AssignmentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newSubmission = viewModel.NewSubmissionViewModel;

                string userId = _userManager.GetUserId(User);
                Assignment assignment = _context.Assignments.Find(newSubmission.TaskVariantId, userId);

                if (assignment.UserId != userId)
                    return Challenge();

                string submissionRoot = Path.Combine(_env.WebRootPath, "Submissions", userId, Guid.NewGuid().ToString());

                _context.Submissions.Add(new Submission()
                {
                    Assignment = assignment,
                    Path = submissionRoot,
                    // TODO: change to UTC
                    Time = DateTime.Now
                });

                if (assignment.Status != ReviewStatus.Graded)
                    assignment.Status = ReviewStatus.Submitted;

                if (!Directory.Exists(submissionRoot))
                    Directory.CreateDirectory(submissionRoot);

                if (newSubmission.Files.All(file => ArchiveExtensions.Contains(new FileInfo(file.FileName).Extension.ToLower())))
                {
                    foreach (var file in newSubmission.Files)
                    {
                        using (var stream = file.OpenReadStream())
                        using (var reader = ReaderFactory.Open(stream))
                            reader.WriteAllToDirectory(submissionRoot, new ExtractionOptions()
                            {
                                ExtractFullPath = true
                            });
                    }
                }
                else
                {
                    foreach (var file in newSubmission.Files)
                        using (var writer = System.IO.File.OpenWrite(Path.Combine(submissionRoot, file.FileName)))
                            file.CopyTo(writer);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("View", new { id = viewModel.NewSubmissionViewModel.TaskVariantId });
        }

        [Authorize]
        public IActionResult Personal()
        {
            var userId = _userManager.GetUserId(User);
            return View(new PersonalViewModel()
            {
                Assignments = _context.Assignments
                .Where(assignment => assignment.UserId == userId)
                .Select(assignment => new AssignmentViewModel()
                {
                    TaskVariantId = assignment.TaskVariantId,
                    UserId = userId,
                    TaskName = assignment.TaskVariant.Name,
                    Score = assignment.Score,
                    Status = assignment.Status
                }).ToList()
            });
        }

        [Authorize]
        public IActionResult PendingReview()
        {
            string userId = _userManager.GetUserId(User);
            var assignments = _context.Assignments
                .Where(assignment => assignment.ReviewerId == userId && assignment.Status == ReviewStatus.Submitted)
                .Select(assignment => new PendingReviewViewModel.PendingReviewItem()
                {
                    UserId = assignment.UserId,
                    TaskVariantId = assignment.TaskVariantId,
                    TaskVariantName = assignment.TaskVariant.Name,
                    UserName = assignment.User.FullName
                }).ToList();

            return View(new PendingReviewViewModel()
            {
                Assignments = assignments
            });
        }

        [HttpGet]
        [Route("[controller]/[action]/{variantId}/{userId}")]
        [Authorize]
        public IActionResult Review(int variantId, string userId)
        {
            var assignment = _context.Assignments.Find(variantId, userId);
            var currentUserId = _userManager.GetUserId(User);

            if (assignment == null)
                return NotFound();

            if (assignment.ReviewerId != currentUserId)
                return Challenge();

            var viewModel = _context.Assignments
                .Where(assign => assign.TaskVariantId == variantId && assign.UserId == userId)
                .Select(assign => new ReviewViewModel()
                {
                    TaskGroupName = assign.TaskVariant.TaskGroup.Name,
                    TaskVariantName = assign.TaskVariant.Name,
                    VaraintId = variantId,
                    UserId = userId,
                    UserName = assign.User.FullName,
                    Status = assign.Status,
                    Score = assign.Score,
                    Submissions = assign.Submissions.Select(submission => new SubmissionViewModel()
                    {
                        Id = submission.Id,
                        Time = submission.Time
                    }).ToList()
                }).First();

            return View(viewModel);
        }

        [HttpPost]
        [Route("[controller]/[action]/{variantId}/{userId}")]
        [Authorize]
        public IActionResult Review(int variantId, string userId, double Score)
        {
            var assignment = _context.Assignments.Find(variantId, userId);
            assignment.Score = Score;
            assignment.Status = ReviewStatus.Graded;
            _context.SaveChanges();
            return RedirectToAction("Review", "Assignment", new { variantId, userId });
        }

        [Authorize]
        public IActionResult Export(int id)
        {
            string[][] table = _context.TaskVariants
                .Where(taskVariant => taskVariant.Id == id)
                .Select(taskVariant => taskVariant.Assignments.Select(assignment => new []
                {
                    taskVariant.Name,
                    assignment.User.FullName,
                    assignment.Status == ReviewStatus.Graded
                        ? assignment.Score.ToString(CultureInfo.InvariantCulture)
                        : "-"
                }).ToArray())
                .FirstOrDefault();
            const string header = "Вариант;Имя;Оценка;";
            string csv = string.Join(Environment.NewLine, new[] {header}.Concat(table.Select(row => string.Join(";", row))));
            byte[] result = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(result, "application/csv", "report.csv");
        }
    }
}
