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

namespace KaCake.ControllersLogic
{
    public class SubmissionLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        public SubmissionViewModel GetSubmission(string userId, int submissionId)
        {
            var submission = _context.Submissions
                .Include(sub => sub.Assignment)
                .FirstOrDefault(sub => sub.Id == submissionId);

            if(submission == null)
            {
                throw new NotFoundException();
            }

            if(submission.Assignment.UserId != userId)
            {
                throw new IllegalAccessException();
            }

            SubmissionViewModel viewModel = new SubmissionViewModel()
                {
                    Id = submission.Id,
                    Time = submission.Time
                };

            return viewModel;
        }
        
        public int DeleteSubmssion(string userId, int id)
        {
            var submission = _context.Submissions
                .Include(sub => sub.Assignment)
                .FirstOrDefault(sub => sub.Id == id);

            if(submission == null)
            {
                throw new NotFoundException();
            }
            if(submission.Assignment.UserId != userId)
            {
                throw new IllegalAccessException();
            }

            var toDelete = new
                {
                    Submission = submission,
                    submission.Assignment,
                    SubmissionsCount = submission.Assignment.Submissions.Count
                };

            Directory.Delete(toDelete.Submission.Path, true);

            _context.Submissions.Remove(toDelete.Submission);

            if (toDelete.Assignment.Status != ReviewStatus.Graded && toDelete.SubmissionsCount <= 1)
                toDelete.Assignment.Status = ReviewStatus.Assigned;

            _context.SaveChanges();

            return toDelete.Assignment.TaskVariantId;
        }
    }
}
