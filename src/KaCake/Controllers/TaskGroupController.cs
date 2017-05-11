using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.TaskVariant;
using KaCake.Utils;
using Microsoft.AspNetCore.Identity;
using KaCake.ControllersLogic;

namespace KaCake.Controllers
{
    public class TaskGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TaskGroupLogic _taskGroupLogic;

        public TaskGroupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            _taskGroupLogic = new TaskGroupLogic(context, userManager);
        }

        [Authorize]
        [Route("api/[controller]/[action]/{taskGroupId}")]
        public IActionResult GetTaskGroup(int taskGroupId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskGroupLogic.GetTaskGroup(userId, taskGroupId));
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

        [HttpGet]
        public IActionResult Create(int id, int? taskGroupId)
        {
            if (taskGroupId.HasValue)
            {
                try
                {
                    return View(_taskGroupLogic.GetTaskGroup(_userManager.GetUserId(User), taskGroupId.Value));
                }
                catch(NotFoundException)
                {
                    return NotFound();
                }
            }

            return View(new TaskGroupViewModel()
            {
                CourseId = id
            });
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/[action]")]
        public IActionResult CreateTaskGroup([FromBody] TaskGroupViewModel taskGroup)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskGroupLogic.CreateTaskGroup(userId, taskGroup));
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
        [HttpPost]
        [Route("api/[controller]/[action]/{taskGroupId}")]
        public IActionResult EditTaskGroup(int taskGroupId, [FromBody] TaskGroupViewModel taskGroup)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                return new ObjectResult(_taskGroupLogic.EditTaskGroup(userId, taskGroupId, taskGroup));
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

        [HttpPost]
        public IActionResult Create(TaskGroupViewModel taskGroup)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                TaskGroup editedTaskGroup;
                if ((editedTaskGroup = _context.TaskGroups.Find(taskGroup.Id)) != null)
                {
                    try
                    {
                        taskGroup = _taskGroupLogic.EditTaskGroup(userId, taskGroup.Id, taskGroup);
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
                else
                {
                    try
                    {
                        taskGroup = _taskGroupLogic.CreateTaskGroup(userId, taskGroup);
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

                return RedirectToAction("View", new { id = taskGroup.Id });
            }

            return View(taskGroup);
        }

        [Authorize]
        public IActionResult View(int id)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                var viewModel = _taskGroupLogic.GetTaskGroup(userId, id);
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
    }
}
