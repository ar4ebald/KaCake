using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KaCake.Data;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.Course;
using KaCake.ViewModels.TaskVariant;
using IndexViewModel = KaCake.ViewModels.Home.IndexViewModel;
using KaCake.Data.Models;

namespace KaCake.ControllersLogic
{
    [Route("api/[controller]")]
    public class HomeLogic
    {
        private readonly ApplicationDbContext _context;

        public HomeLogic(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
