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
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.ControllersLogic
{
    public class ProjectLogic
    {
        private const string CommentsFileExtension = ".kacakecomments.json";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public IndexViewModel GetProject(string userId, int submissionId)
        {
            var submission = _context.Submissions
                .Include(sub => sub.Assignment)
                .Include(sub => sub.Assignment.User)
                .Include(sub => sub.Assignment.TaskVariant)
                .Include(sub => sub.Assignment.TaskVariant.TaskGroup)
                .Where(sub => sub.Id == submissionId 
                && (sub.Assignment.UserId == userId || sub.Assignment.ReviewerId == userId))
                .FirstOrDefault();

            if(submission == null)
            {
                throw new NotFoundException();
            }

            var viewModel = new
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
                };

            viewModel.Model.Root = GetEntires(new DirectoryInfo(viewModel.RootPath));

            return viewModel.Model;
        }

        [Authorize]
        public FileViewModel GetFileContent(string userId, int submissionId, [FromQuery]string file)
        {
            var submissions = _context.Submissions
                 .Where(submission => submission.Id == submissionId
                    && (submission.Assignment.UserId == userId || submission.Assignment.ReviewerId == userId));

            string root = submissions
                .Select(submission => submission.Path)
                .FirstOrDefault();

            if (root == null || file == null)
                throw new NotFoundException();

            string path = Path.Combine(root, file);
            if (!System.IO.File.Exists(path))
                throw new NotFoundException();

            string commentsPath = path + CommentsFileExtension;
            if (System.IO.File.Exists(commentsPath))
            {
                return new FileViewModel
                {
                    Text = System.IO.File.ReadAllText(path),
                    Comments = System.IO.File.ReadAllText(commentsPath)
                };
            }

            return new FileViewModel
            {
                Text = System.IO.File.ReadAllText(path)
            };
        }

        [Authorize]
        public List<object> GetAllComments(string userId, int submissionId)
        {
            var submissions = _context.Submissions
                 .Where(submission => submission.Id == submissionId
                    && (submission.Assignment.UserId == userId || submission.Assignment.ReviewerId == userId));

            string root = submissions
                .Select(submission => submission.Path)
                .FirstOrDefault();

            if(root == null)
            {
                throw new NotFoundException();
            }

            if(!System.IO.Directory.Exists(root))
            {
                throw new NotFoundException();
            }
            
            string[] commentsFiles = Directory.GetFiles(root, "*" + CommentsFileExtension, SearchOption.AllDirectories);
            List<object> commentsList = new List<object>();
            foreach(string file in commentsFiles)
            {
                string commentsAllText = System.IO.File.ReadAllText(file);
                var comments = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<CommentViewModel>>(commentsAllText);
                foreach(var c in comments)
                {
                    c.File = file.Substring(root.Length + 1, file.Length - root.Length - CommentsFileExtension.Length - 1);
                }

                commentsList.AddRange(comments);
            }
            
            return commentsList;
        }
        
        public bool SaveComments(string userId, int submissionId, string file, string commentsJson)
        {
            var submission = _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.TaskVariant)
                .Include(s => s.Assignment.TaskVariant.TaskGroup)
                .FirstOrDefault(s => s.Id == submissionId);

            string root = submission.Path;

            if (root == null || file == null || commentsJson == null)
                throw new NotFoundException();

            string path = Path.Combine(root, file);
            if (!System.IO.File.Exists(path))
                throw new NotFoundException();

            string jsonPath = path + CommentsFileExtension;
            System.IO.File.WriteAllText(jsonPath, commentsJson);

            return true;
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

