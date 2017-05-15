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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KaCake.ControllersLogic
{
    public class AssignmentLogic
    {
        private static readonly HashSet<string> ArchiveExtensions = new HashSet<string>(new[]
        {
            ".zip", ".rar", ".tar", ".7z"
        });

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _env;

        public AssignmentLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [Authorize]
        public IList<AssignmentViewModel> GetAssignmentsForTaskVariant(int taskVariantId)
        {
            if(_context.TaskVariants.Find(taskVariantId) == null)
            {
                throw new NotFoundException();
            }

            return _context.Assignments
                   .Where(assignment => assignment.TaskVariantId == taskVariantId)
                   .Select(assignment => new AssignmentViewModel()
                   {
                       TaskVariantId = taskVariantId,
                       UserId = assignment.UserId,
                       UserName = assignment.User.FullName,
                       Score = assignment.Score,
                       Status = assignment.Status
                   }).ToList();
        }

        public AssignmentViewModel GetAssignment(string userId, int taskVariantId)
        {
            AssignmentViewModel viewingAssignment = _context.Assignments
                .Include(assignment => assignment.TaskVariant)
                .Include(assignment => assignment.TaskVariant.Assignments)
                .Where(assignment => assignment.UserId == userId && assignment.TaskVariantId == taskVariantId)
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
                        Time = submission.Time,
                        Status = submission.Status,
                        ReviewTitle = submission.ReviewTitle,
                        ReviewMessage = submission.ReviewMessage
                    }).ToList(),
                    NewSubmissionViewModel = new AddSubmissionViewModel()
                    {
                        TaskVariantId = taskVariantId
                    },
                    Score = assignment.Score,
                    Status = assignment.Status
                }).FirstOrDefault();

            if (viewingAssignment == null)
                throw new NotFoundException();

            return viewingAssignment;
        }

        public AssignmentViewModel AddSubmission(ModelStateDictionary modelState, string userId, AssignmentViewModel viewModel)
        {
            if (modelState.IsValid)
            {
                var newSubmission = viewModel.NewSubmissionViewModel;

                Assignment assignment = _context.Assignments.Find(newSubmission.TaskVariantId, userId);

                if (assignment.UserId != userId)
                    throw new IllegalAccessException();

                string submissionRoot = Path.Combine(_env.WebRootPath, "Submissions", userId, Guid.NewGuid().ToString());
                string testerPath = Path.Combine(_env.WebRootPath, "App_Data", "Testers", newSubmission.TaskVariantId.ToString());
                bool testerExists = System.IO.File.Exists(testerPath);

                _context.Submissions.Add(new Submission()
                {
                    Assignment = assignment,
                    Path = submissionRoot,
                    // TODO: change to UTC
                    Time = DateTime.Now,
                    Status = testerExists ? TestingStatus.Pending : TestingStatus.Testing
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

            // use viewModel.NewSubmissionViewModel.TaskVariantId 
            return viewModel;
        }

        public PersonalViewModel Personal(string userId)
        {
            return new PersonalViewModel()
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
            };
        }

        public PendingReviewViewModel PendingReview(string userId)
        {
            var assignments = _context.Assignments
                .Where(assignment => assignment.ReviewerId == userId && assignment.Status == ReviewStatus.Submitted)
                .Select(assignment => new PendingReviewViewModel.PendingReviewItem()
                {
                    UserId = assignment.UserId,
                    TaskVariantId = assignment.TaskVariantId,
                    TaskVariantName = assignment.TaskVariant.Name,
                    UserName = assignment.User.FullName
                }).ToList();

            return new PendingReviewViewModel()
            {
                Assignments = assignments
            };
        }

        public ReviewViewModel Review(string currentUserId, int variantId, string userId)
        {
            var assignment = _context.Assignments.Find(variantId, userId);

            if (assignment == null)
                throw new NotFoundException();

            if (assignment.ReviewerId != currentUserId)
                throw new IllegalAccessException();

            var viewModel = _context.Assignments
                .Include(ass => ass.TaskVariant)
                .Include(ass => ass.User)
                .Include(ass => ass.Submissions)
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

            return viewModel;
        }

        public bool Review(string currentUserId, int variantId, string userId, double Score)
        {
            var assignment = _context.Assignments.Find(variantId, userId);

            if (assignment == null)
                throw new NotFoundException();

            if (assignment.ReviewerId != currentUserId)
                throw new IllegalAccessException();

            assignment.Score = Score;
            assignment.Status = ReviewStatus.Graded;
            _context.SaveChanges();
            //return RedirectToAction("Review", "Assignment", new { variantId, userId });
            return true;
        }

        public byte[] Export(int id)
        {
            string[][] table = _context.TaskVariants
                .Where(taskVariant => taskVariant.Id == id)
                .Select(taskVariant => taskVariant.Assignments.Select(assignment => new[]
                {
                    taskVariant.Name,
                    assignment.User.FullName,
                    assignment.Status == ReviewStatus.Graded
                        ? assignment.Score.ToString(CultureInfo.InvariantCulture)
                        : "-"
                }).ToArray())
                .FirstOrDefault();

            if(table == null)
            {
                throw new NotFoundException();
            }

            const string header = "Вариант;Имя;Оценка;";
            string csv = string.Join(Environment.NewLine, new[] { header }.Concat(table.Select(row => string.Join(";", row))));
            byte[] result = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            //return File(result, "application/csv", "report.csv");
            return result;
        }
    }
}
