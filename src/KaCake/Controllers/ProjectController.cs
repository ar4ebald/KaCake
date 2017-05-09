using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Project;
using KaCake.Utils;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class ProjectController : Controller
    {
        private const string CommentsFileExtension = ".kacakecomments.json";

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
            string userId = _userManager.GetUserId(User);

            var submissions = _context.Submissions
                .Where(submission => submission.Id == id 
                && (submission.Assignment.UserId == userId || submission.Assignment.ReviewerId == userId));

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
                        SubmissionId = submission.Id,
                        IsCourseTeacher = KaCakeUtils.IsCourseTeacher(_context, submission.Assignment.TaskVariant.TaskGroup.CourseId, userId)
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
            string userId = _userManager.GetUserId(User);

            var submissions = _context.Submissions
                 .Where(submission => submission.Id == id
                    && (submission.Assignment.UserId == userId || submission.Assignment.ReviewerId == userId));

            string root = submissions
                .Select(submission => submission.Path)
                .FirstOrDefault();

            if (root == null || file == null)
                return NotFound();

            string path = Path.Combine(root, file);
            if (!System.IO.File.Exists(path))
                return NotFound();

            string commentsPath = path + CommentsFileExtension;
            if (System.IO.File.Exists(commentsPath))
            {
                return Json(new
                {
                    Text = System.IO.File.ReadAllText(path),
                    Comments = System.IO.File.ReadAllText(commentsPath)
                });
            }

            return Json(new
            {
                Text = System.IO.File.ReadAllText(path)
            });
        }

        [Authorize]
        public IActionResult GetAllComments(int id)
        {
            string userId = _userManager.GetUserId(User);

            var submissions = _context.Submissions
                 .Where(submission => submission.Id == id
                    && (submission.Assignment.UserId == userId || submission.Assignment.ReviewerId == userId));

            string root = submissions
                .Select(submission => submission.Path)
                .FirstOrDefault();

            if(root == null)
            {
                return NotFound();
            }

            if(!System.IO.Directory.Exists(root))
            {
                return NotFound();
            }

            string[] commentsFiles = Directory.GetFiles(root, "*" + CommentsFileExtension, SearchOption.AllDirectories);
            List<object> commentsList = new List<object>();
            foreach(string file in commentsFiles)
            {
                string comments = System.IO.File.ReadAllText(file);
                var jarr = Newtonsoft.Json.Linq.JArray.Parse(comments);
                commentsList.AddRange(jarr);
            }
            
            return Json(commentsList);
        }
        
        public IActionResult SaveComments(int id, string file, string commentsJson)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            var submission = _context.Submissions.Find(id);

            if(!KaCakeUtils.IsCourseTeacher(_context, submission.Assignment.TaskVariant.TaskGroup.CourseId, userId))
            {
                return Challenge();
            }

            string root = submission.Path;

            if (root == null || file == null || commentsJson == null)
                return NotFound();

            string path = Path.Combine(root, file);
            if (!System.IO.File.Exists(path))
                return NotFound();

            string jsonPath = path + CommentsFileExtension;
            System.IO.File.WriteAllText(jsonPath, commentsJson);

            return Ok();
        }

        private static IndexViewModel.FileSystemEntry GetEntires(DirectoryInfo directory)
        {
            return new IndexViewModel.FileSystemEntry()
            {
                Name = directory.Name,
                IsDirectory = true,
                SubEntries = directory.EnumerateDirectories()
                    .Select(GetEntires)
                    .Concat(directory.EnumerateFiles().Where(file => !file.Name.EndsWith(CommentsFileExtension)).Select(file => new IndexViewModel.FileSystemEntry()
                    {
                        Name = Path.GetFileName(file.Name),
                        IsDirectory = false
                    }))
                    .ToList()
            };
        }
    }
}

