using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult Index(int id)
        {
            var submissions = _context.Submissions
                .Where(submission => submission.Id == id);

            if (!User.IsInRole(RoleNames.Admin))
            {
                string userId = _userManager.GetUserId(User);
                submissions = submissions.Where(submission => submission.Assignment.UserId == userId);
            }

            var viewModel = submissions
                .Select(submission => new
                {
                    RootPath = submission.Path,
                    Model = new IndexViewModel()
                    {
                        UserName = submission.Assignment.User.FullName,
                        TaskGroupName = submission.Assignment.TaskVariant.TaskGroup.Name,
                        TaskVariantName = submission.Assignment.TaskVariant.Name,
                        SubmissionTime = submission.Time,
                        SubmissionId = submission.Id
                    }
                }).FirstOrDefault();

            if (viewModel == null)
                return NotFound();

            viewModel.Model.Root = GetEntires(new DirectoryInfo(viewModel.RootPath));

            return View(viewModel.Model);
        }

        [Authorize]
        public IActionResult GetFile(int id, [FromQuery]string file)
        {
            var submissions = _context.Submissions
                .Where(submission => submission.Id == id);

            if (!User.IsInRole(RoleNames.Admin))
            {
                string userId = _userManager.GetUserId(User);
                submissions = submissions.Where(submission => submission.Assignment.UserId == userId);
            }

            string root = submissions
                .Select(submission => submission.Path)
                .FirstOrDefault();

            if (root == null || file == null)
                return NotFound();

            string path = Path.Combine(root, file);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return Content(System.IO.File.ReadAllText(path));
        }

        private static IndexViewModel.FileSystemEntry GetEntires(DirectoryInfo directory)
        {
            return new IndexViewModel.FileSystemEntry()
            {
                Name = directory.Name,
                IsDirectory = true,
                SubEntries = directory.EnumerateDirectories()
                    .Select(GetEntires)
                    .Concat(directory.EnumerateFiles().Select(file => new IndexViewModel.FileSystemEntry()
                    {
                        Name = Path.GetFileName(file.Name),
                        IsDirectory = false
                    }))
                    .ToList()
            };
        }
    }
}

