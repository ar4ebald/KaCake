using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Submission;

namespace KaCake.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult View(int id)
        {
            string userId = _userManager.GetUserId(User);

            SubmissionViewModel viewModel = _context.Submissions
                .Where(submission => submission.Id == id && submission.Assignment.UserId == userId)
                .Select(submission => new SubmissionViewModel()
                {
                    Id = submission.Id,
                    Time = submission.Time,
                    Status = submission.Status,
                    ReviewTitle = submission.ReviewTitle,
                    ReviewMessage = submission.ReviewMessage
                }).FirstOrDefault();

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            string userId = _userManager.GetUserId(User);

            var toDelete = _context.Submissions
                .Include(submission => submission.Assignment)
                .Where(submission => submission.Assignment.UserId == userId && submission.Id == id)
                .Select(submission => new
                {
                    Submission = submission,
                    submission.Assignment,
                    SubmissionsCount = submission.Assignment.Submissions.Count
                })
                .FirstOrDefault();

            if (toDelete == null)
                return NotFound();

            try
            {
                Directory.Delete(toDelete.Submission.Path, true);
            }
            catch (IOException) { }

            _context.Submissions.Remove(toDelete.Submission);

            if (toDelete.Assignment.Status != ReviewStatus.Graded && toDelete.SubmissionsCount <= 1)
                toDelete.Assignment.Status = ReviewStatus.Assigned;

            _context.SaveChanges();

            return RedirectToAction("View", "Assignment", new { id = toDelete.Assignment.TaskVariantId });
        }
    }
}
