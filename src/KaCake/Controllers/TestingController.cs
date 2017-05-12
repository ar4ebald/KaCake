using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KaCake.Data;
using KaCake.Data.Migrations;
using KaCake.Data.Models;
using KaCake.ViewModels.Submission;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers;

namespace KaCake.Controllers
{
    public class TestingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _env;

        public TestingController(ApplicationDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult GetPendingSubmission()
        {
            var submission = _context.Submissions
                .Include(i => i.Assignment)
                .FirstOrDefault(i => i.Status == TestingStatus.Pending);

            if (submission == null)
                return NoContent();

            submission.PickedForTestingTimeUtc = DateTime.UtcNow;
            submission.Status = TestingStatus.Testing;

            _context.SaveChanges();

            return Json(new
            {
                SubmissionId = submission.Id,
                submission.Assignment.TaskVariantId
            });
        }

        public IActionResult GetTester(int id)
        {
            var tester = new FileInfo(Path.Combine(_env.WebRootPath, "App_Data", "Testers", id.ToString()));

            if (tester.Exists)
                return File(tester.OpenRead(), "application/octet-stream");
            else
                return NotFound();
        }

        public IActionResult GetSubmission(int id)
        {
            string path =
                _context.Submissions
                    .Where(i => i.Id == id)
                    .Select(i => i.Path)
                    .FirstOrDefault();

            if (Directory.Exists(path))
            {
                using (var zip = ZipArchive.Create())
                {
                    zip.AddAllFromDirectory(path);
                    using (var memory = new MemoryStream())
                    {
                        zip.SaveTo(memory);
                        return File(memory.ToArray(), "application/octet-stream", "submission.zip");
                    }
                }
            }

            return NotFound();
        }

        public IActionResult SetStatus(int submissionId, double score, string title, string message)
        {
            Submission submission = _context.Submissions
                .Include(i => i.Assignment)
                .FirstOrDefault(i => i.Id == submissionId);

            if (submission == null)
                return NotFound();

            if (title == "OK")
            {
                submission.Status = TestingStatus.Passed;
                submission.Assignment.Status = ReviewStatus.Graded;
                submission.Assignment.Score = score;
            }
            else
                submission.Status = TestingStatus.Failed;

            submission.ReviewTitle = title;
            submission.ReviewMessage = message;

            _context.SaveChanges();

            return Ok();
        }
    }
}