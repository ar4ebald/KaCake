using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KaCake.Data;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.Course;
using KaCake.ViewModels.TaskVariant;
using IndexViewModel = KaCake.ViewModels.Home.IndexViewModel;
using KaCake.Data.Models;

namespace KaCake.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new ViewModels.Home.IndexViewModel()
            {
                ViewModel = null,
                SubTree = _context.Courses.Select(course => new ViewModels.Home.IndexViewModel()
                {
                    ViewModel = new CourseViewModel()
                    {
                        Id = course.Id,
                        Name = course.Name
                    },
                    SubTree = course.TaskGroups.Select(taskGroup => new ViewModels.Home.IndexViewModel()
                    {
                        ViewModel = new TaskGroupViewModel()
                        {
                            Id = taskGroup.Id,
                            Name = taskGroup.Name
                        },
                        SubTree = taskGroup.Variants.Select(taskVariant => new ViewModels.Home.IndexViewModel()
                        {
                            ViewModel = new TaskVariantViewModel()
                            {
                                Id = taskVariant.Id,
                                Name = taskVariant.Name
                            }
                        }).ToList()
                    }).ToList()
                }).ToList()
            });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
