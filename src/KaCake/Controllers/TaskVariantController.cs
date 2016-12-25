using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.TaskGroup;
using KaCake.ViewModels.TaskVariant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KaCake.Controllers
{
    public class TaskVariantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TaskVariantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize]
        public IActionResult View(int id)
        {
            string userId = _userManager.GetUserId(User);

            TaskVariantViewModel viewModel = _context.TaskVariants
                .Where(taskVariant => taskVariant.Id == id)
                .Select(taskVariant => new TaskVariantViewModel()
                {
                    Id = id,
                    Name = taskVariant.Name,
                    Description = taskVariant.Description,
                    AssignmentsCount = taskVariant.Assignments.Count,
                    IsAssigned = taskVariant.Assignments.Any(assignment => assignment.UserId == userId)
                }).FirstOrDefault();

            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(int id)
        {
            return View(new TaskVariantViewModel
            {
                TaskGroupId = id
            });
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Create(TaskVariantViewModel taskVariant)
        {
            if (ModelState.IsValid)
            {
                EntityEntry<TaskVariant> addedVariant = _context.TaskVariants.Add(new TaskVariant()
                {
                    TaskGroupId = taskVariant.TaskGroupId,
                    Name = taskVariant.Name,
                    Description = taskVariant.Description,
                });
                _context.SaveChanges();
                return RedirectToAction(nameof(View), new { id = addedVariant.Entity.Id });
            }

            return View(taskVariant);
        }

        private void PopulateAddAssignmentData(List<SelectListItem> usersToRemove)
        {
            var usersToRemoveIds = new HashSet<string>(usersToRemove.Select(user => user.Value));

            var roleId = _context.Roles.First(role => role.Name == RoleNames.Admin).Id;
            ViewData["Reviewers"] = _context.Users.Where(user => user.Roles.Select(role => role.RoleId).Contains(roleId))
                .Select(user => new SelectListItem
                {
                    Text = user.UserName ?? user.Email,
                    Value = user.Id
                }).ToList();

            ViewData["UsersToAdd"] = _context.Users
                .Where(user => !usersToRemoveIds.Contains(user.Id))
                .Select(user => new SelectListItem()
                {
                    Text = user.UserName ?? user.Email,
                    Value = user.Id
                }).ToList();

            ViewData["UsersToRemove"] = usersToRemove;

        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult AddAssignments(int id)
        {
            var editingVariant = _context.TaskVariants
                .Where(variant => variant.Id == id)
                .Select(variant => new
                {
                    variant.Id,
                    variant.Name,
                    UsersToRemove =
                    variant.Assignments.Select(assignment => new
                    {
                        assignment.User.Id,
                        Name = assignment.User.UserName ?? assignment.User.Email
                    })
                })
                .FirstOrDefault();

            if (editingVariant == null)
                return NotFound();

            var usersToRemove = editingVariant.UsersToRemove.Select(user => new SelectListItem()
            {
                Text = user.Name,
                Value = user.Id
            }).ToList();
            PopulateAddAssignmentData(usersToRemove);

            return View(new AddAsignmentsViewModel()
            {
                TaskVariantId = editingVariant.Id,
                TaskVariantName = editingVariant.Name,
                DeadlineUtc = DateTime.Now,
            });
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult AddAssignments(AddAsignmentsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var reviewer = _context.Users.Find(viewModel.ReviewerId);
                if (reviewer == null)
                {
                    ModelState.AddModelError("Wrong reviewer", "Wrong reviewer");
                }
                else
                {
                    if (viewModel.UsersToRemove != null)
                    {
                        _context.Assignments.RemoveRange(
                            _context.Assignments.Where(
                                assignment => viewModel.UsersToRemove.Contains(assignment.UserId)
                            )
                        );
                    }

                    if (viewModel.UsersToAdd != null)
                    {
                        _context.Assignments.AddRange(viewModel.UsersToAdd.Select(user => new Assignment()
                        {
                            TaskVariantId = viewModel.TaskVariantId,
                            DeadlineUtc = viewModel.DeadlineUtc,
                            UserId = user,
                            ReviewerId = reviewer.Id
                        }));
                    }

                    _context.SaveChanges();
                    return RedirectToAction("View", new { id = viewModel.TaskVariantId });
                }
            }

            var editingVariant = _context.TaskVariants
                .Where(variant => variant.Id == viewModel.TaskVariantId)
                .Select(variant => variant.Assignments.Select(assignment => new
                {
                    assignment.User.Id,
                    Name = assignment.User.UserName ?? assignment.User.Email
                })
                )
                .FirstOrDefault();

            var usersToRemove = editingVariant.Select(user => new SelectListItem()
            {
                Text = user.Name,
                Value = user.Id
            }).ToList();
            PopulateAddAssignmentData(usersToRemove);

            return View(viewModel);
        }
    }
}
