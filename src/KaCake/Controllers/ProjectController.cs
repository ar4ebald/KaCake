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
using KaCake.ControllersLogic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.Controllers
{
    public class ProjectController : Controller
    {
        private const string CommentsFileExtension = ".kacakecomments.json";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ProjectLogic _projectLogic;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            _projectLogic = new ProjectLogic(context, userManager);
        }

        [Authorize]
        [Route("[controller]/[action]/{submissionId}")]
        public IActionResult Index(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                var viewModel = _projectLogic.GetProject(userId, submissionId);
                return View(viewModel);
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{submissionId}")]
        public IActionResult GetProject(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                return new ObjectResult(_projectLogic.GetProject(userId, submissionId));
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("[controller]/[action]/{submissionId}")]
        public IActionResult GetFile(int submissionId, [FromQuery]string file)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                var fileVm = _projectLogic.GetFileContent(userId, submissionId, file);
                return Json(fileVm);
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{submissionId}/{file}")]
        public IActionResult GetFileContent(int submissionId, string file)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                return new ObjectResult(_projectLogic.GetFileContent(userId, submissionId, file));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        public IActionResult GetAllComments(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                var commentsList = _projectLogic.GetAllComments(userId, submissionId);
                return Json(commentsList);
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]/{submissionId}")]
        public IActionResult GetAllProjectComments(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                return new ObjectResult(_projectLogic.GetAllComments(userId, submissionId));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
        
        [Authorize]
        public IActionResult SaveComments(int submissionId, string file, string commentsJson)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                if (_projectLogic.SaveComments(userId, submissionId, file, commentsJson))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
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
        [Route("api/[controller]/[action]/{submissionId}/{file}")]
        public IActionResult SaveFileComments(int submissionId, string file, [FromQuery] string commentsJson)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                return new ObjectResult(_projectLogic.SaveComments(userId, submissionId, file, commentsJson));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch(IllegalAccessException)
            {
                return Challenge();
            }
        }
    }
}

