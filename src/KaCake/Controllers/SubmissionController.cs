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
using KaCake.ControllersLogic;

namespace KaCake.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SubmissionLogic _submissionLogic;

        public SubmissionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            _submissionLogic = new SubmissionLogic(context, userManager);
        }

        [Authorize]
        [Route("[controller]/[action]/{submissionId}")]
        public IActionResult View(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                SubmissionViewModel viewModel = _submissionLogic.GetSubmission(userId, submissionId);
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
        [Route("api/[controller]/[action]/{submissionId}")]
        public IActionResult GetAssignment(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                SubmissionViewModel viewModel = _submissionLogic.GetSubmission(userId, submissionId);
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
        [Route("[controller]/[action]/{submissionId}")]
        public IActionResult Delete(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                int result = _submissionLogic.DeleteSubmssion(userId, submissionId);
                if (result != -1)
                {
                    return RedirectToAction("View", "Assignment", new { id = result });
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

        [Authorize]
        [Route("api/[controller]/[action]/{submissionId}")]
        public IActionResult DeleteAssignment(int submissionId)
        {
            string userId = _userManager.GetUserId(User);

            try
            {
                int result = _submissionLogic.DeleteSubmssion(userId, submissionId);
                if (result != -1)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
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

    }
}
