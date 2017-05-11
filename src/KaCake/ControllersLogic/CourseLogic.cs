using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KaCake.ViewModels.TaskGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KaCake.Data;
using KaCake.Data.Models;
using KaCake.ViewModels.Assignment;
using KaCake.ViewModels.Course;
using IndexViewModel = KaCake.ViewModels.Course.IndexViewModel;
using KaCake.Utils;
using KaCake.ViewModels.UserInfo;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaCake.ControllersLogic
{
    public class CourseLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseLogic(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<CourseViewModel> GetAllCourses()
        {
            return _context.Courses
                    .Select(course => new CourseViewModel()
                    {
                        Id = course.Id,
                        Name = course.Name
                    })
                    .ToList();
        }
        
        public CourseViewModel GetCourse(string callerId, int courseId)
        {
            var viewingCourse = _context.Courses
                .Include(course => course.TaskGroups)
                .Include(course => course.Teachers)
                .Include(course => course.Students)
                .FirstOrDefault(course => course.Id == courseId);

            if (viewingCourse == null)
                throw new NotFoundException();
            
            return new CourseViewModel()
            {
                Id = viewingCourse.Id,
                Name = viewingCourse.Name,
                Description = viewingCourse.Description,
                TaskGroups = viewingCourse.TaskGroups.Select(taskGroup => new TaskGroupViewModel()
                {
                    Id = taskGroup.Id,
                    Name = taskGroup.Name
                }).ToList(),
                Students = viewingCourse.Students.Select(
                        student => KaCakeUtils.createUserInfoViewModel(_context, student.UserId))
                    .ToList(),
                Teachers = viewingCourse.Teachers.Select(
                        teacher => KaCakeUtils.createUserInfoViewModel(_context, teacher.TeacherId))
                    .ToList(),
                IsUserATeacher = viewingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId)
            };
        }
        
        public IList<UserInfoViewModel> GetTeachersCouldBeAdded(string callerId)
        {
            return _context.Users.Select(user =>
            new UserInfoViewModel
            {
                FullName = user.FullName,
                UserName = user.UserName,
                UserId = user.Id
            }).ToList();
        }

        public IList<UserInfoViewModel> GetTeachersCouldBeAdded(string callerId, int courseId)
        {
            var editingCourse =
                    _context.Courses.Include(c => c.Teachers)
                        .FirstOrDefault(c => c.Id == courseId);

            if(editingCourse == null)
            {
                throw new NotFoundException();
            }

            return _context.Users
                .Where(t => !editingCourse.Teachers.Any(teacher => teacher.TeacherId.Equals(t.Id)))
                .Select(user =>
                new UserInfoViewModel
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    UserId = user.Id
                }).ToList();
        }

        public IList<UserInfoViewModel> GetTeachersCouldBeRemoved(string callerId, int courseId)
        {
            var editingCourse =
                    _context.Courses.Include(c => c.Teachers)
                        .FirstOrDefault(c => c.Id == courseId);

            if(editingCourse == null)
            {
                throw new NotFoundException();
            }

            return editingCourse.Teachers
                        .Where(teacher => KaCakeUtils.isAppointer(editingCourse, callerId, teacher))
                        .Select(teacher => new UserInfoViewModel
                        {
                            FullName = _context.Users.Find(teacher.TeacherId).FullName,
                            UserName = _context.Users.Find(teacher.TeacherId).UserName,
                            UserId = teacher.TeacherId
                        })
                        .ToList();
        }

        public CourseViewModel Create(string callerId, ModelStateDictionary modelState, CreateViewModel course)
        {
            if(!modelState.IsValid)
            {
                throw new InvalidModelException();
            }

            Course courseToAdd = new Course()
            {
                Name = course.Name,
                Description = course.Description
            };
            _context.Courses.Add(courseToAdd);
            _context.SaveChanges();

            // And now the id of the course could be obtained
            CourseTeacher2 courseTeacher = new CourseTeacher2
            {
                CourseId = courseToAdd.Id,
                TeacherId = callerId,
                AppointerId = callerId,
            };
            var teachers = new List<CourseTeacher2>();
            teachers.Add(courseTeacher);

            // Add all the 'Teachers to add'
            foreach (string teacherToAddId in course.TeachersToAdd)
            {
                teachers.Add(new CourseTeacher2
                {
                    CourseId = courseToAdd.Id,
                    TeacherId = teacherToAddId,
                    AppointerId = callerId
                });
            }

            // Then set the 'teacher' field
            courseToAdd.Teachers = teachers;

            _context.SaveChanges();

            return GetCourse(callerId, courseToAdd.Id);
        }

        public CourseViewModel Edit(string callerId, ModelStateDictionary modelState, int courseId, CreateViewModel course)
        {
            if(!modelState.IsValid)
            {
                throw new InvalidModelException();
            }

            Course editingCourse;
            if ((editingCourse =
                _context.Courses.Include(c => c.Teachers)
                    .FirstOrDefault(c => c.Id == courseId)) != null)
            {
                // The course could be edited only by a teacher of that course
                if (editingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId))
                {
                    editingCourse.Name = course.Name;
                    editingCourse.Description = course.Description;

                    if (course.TeachersToAdd != null)
                    {
                        course.TeachersToAdd.Select(
                            teacherId => new CourseTeacher2
                            {
                                CourseId = course.Id.GetValueOrDefault(),
                                TeacherId = teacherId,
                                AppointerId = callerId

                            }
                        ).ToList().ForEach(teacher => editingCourse.Teachers.Add(teacher));
                    }

                    if (course.TeachersToRemove != null)
                    {
                        var teachersToRemove = course.TeachersToRemove
                            .Where(userId => editingCourse.Teachers.Any(teacher => teacher.TeacherId.Equals(userId)))
                            .Select(teacherId => editingCourse.Teachers.First(teacher => teacher.TeacherId.Equals(teacherId)));

                        foreach (var teacherToRemove in teachersToRemove)
                        {
                            // Only appointer can remove teachers appointed by him
                            if (KaCakeUtils.isAppointer(editingCourse, callerId, teacherToRemove))
                            {
                                editingCourse.Teachers.Remove(teacherToRemove);
                            }
                            else
                            {
                                throw new IllegalAccessException();
                            }
                        }
                    }
                }

                return GetCourse(callerId, courseId);
            }
            else
            {
                throw new NotFoundException();
            }
        }

    }
}
