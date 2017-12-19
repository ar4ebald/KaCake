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
                IsUserATeacher = viewingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId),
                CanDeleteThisCourse = CanDeleteCourse(callerId, courseId)
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

            if (editingCourse == null)
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

            if (editingCourse == null)
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

        public IList<UserInfoViewModel> GetStudentsCouldBeAdded()
        {
            return _context.Users.Select(user =>
                new UserInfoViewModel
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    UserId = user.Id
                }).ToList();
        }

        public IList<UserInfoViewModel> GetStudentsCouldBeAdded(int courseId)
        {
            var editingCourse =
                _context.Courses.Include(c => c.Students)
                .FirstOrDefault(c => c.Id == courseId);

            if (editingCourse == null)
            {
                throw new NotFoundException();
            }

            var editingCourseStudentsIds = _context.Users
                .Join(editingCourse.Students, u => u.Id, s => s.UserId, (u, s) => u.Id);

            return _context.Users
                .Where(u => !editingCourseStudentsIds.Contains(u.Id))
                .Select(user =>
                new UserInfoViewModel
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    UserId = user.Id
                }).ToList();
        }

        public IList<UserInfoViewModel> GetStudentsCouldBeRemoved(int courseId)
        {
            var editingCourse =
                _context.Courses.Include(c => c.Students)
                .FirstOrDefault(c => c.Id == courseId);

            if (editingCourse == null)
            {
                throw new NotFoundException();
            }

            return _context.Users
                .Join(editingCourse.Students, u => u.Id, s => s.UserId, (u, s) =>
                new UserInfoViewModel
                {
                    FullName = u.FullName,
                    UserName = u.UserName,
                    UserId = u.Id
                }).ToList();
        }

        public CourseViewModel Create(string callerId, CreateViewModel course)
        {
            Course courseToAdd = new Course()
            {
                Name = course.Name,
                Description = course.Description,
                CreatorId = callerId
            };
            _context.Courses.Add(courseToAdd);
            _context.SaveChanges();

            // Then we can set a creator
            courseToAdd.CreatorId = callerId;

            var teachers = new List<CourseTeacher>();

            // Add all the 'Teachers to add'
            if (course.TeachersToAdd != null)
            {
                foreach (string teacherToAddId in course.TeachersToAdd.Concat(new[] { callerId }).Distinct())
                {
                    teachers.Add(new CourseTeacher
                    {
                        CourseId = courseToAdd.Id,
                        TeacherId = teacherToAddId,
                        AppointerId = callerId
                    });
                }
            }

            // Then set the 'teacher' field
            courseToAdd.Teachers = teachers;

            // And the add students
            if (course.StudentsToAdd != null)
            {
                courseToAdd.Students = course.StudentsToAdd
                    .Select(studentId => new CourseEnrollment
                    {
                        UserId = studentId,
                        CourseId = courseToAdd.Id
                    }).ToList();
            }

            _context.SaveChanges();

            return GetCourse(callerId, courseToAdd.Id);
        }

        public CourseViewModel Edit(string callerId, int courseId, CreateViewModel course)
        {
            Course editingCourse;
            if ((editingCourse =_context.Courses.Find(courseId)) != null)
            {
                // The course could be edited only by a teacher of that course or it's creator
                if (editingCourse.Teachers.Any(teacher => teacher.TeacherId == callerId) || editingCourse.CreatorId.Equals(callerId))
                {
                    editingCourse.Name = course.Name;
                    editingCourse.Description = course.Description;

                    if(course.TeachersToAdd != null || course.TeachersToRemove != null)
                    {
                        _context.Entry(editingCourse).Collection(c => c.Teachers).Load();
                    }
                    if(course.StudentsToAdd != null || course.StudentsToRemove != null)
                    {
                        _context.Entry(editingCourse).Collection(c => c.Students).Load();
                    }

                    if (course.TeachersToAdd != null)
                    {
                        var list = course.TeachersToAdd.Select(
                            teacherId => new CourseTeacher
                            {
                                CourseId = course.Id.GetValueOrDefault(),
                                TeacherId = teacherId,
                                AppointerId = callerId
                            }
                        ).ToList();
                        list.ForEach(teacher => editingCourse.Teachers.Add(teacher));
                    }

                    if (course.TeachersToRemove != null)
                    {
                        foreach (var userId in course.TeachersToRemove)
                        {
                            var teacherToRemove = editingCourse.Teachers.FirstOrDefault(t => t.TeacherId.Equals(userId));
                            if(teacherToRemove == null)
                            {
                                continue;
                            }

                            _context.Entry(teacherToRemove).Collection(c => c.AppointedTeachers).Load();

                            // Only appointer can remove teachers appointed by him
                            if (KaCakeUtils.isAppointer(editingCourse, callerId, teacherToRemove))
                            {
                                foreach(var appointedTeacher in teacherToRemove.AppointedTeachers)
                                {
                                    appointedTeacher.AppointerId = callerId;
                                }

                                editingCourse.Teachers.Remove(teacherToRemove);
                            }
                            else
                            {
                                throw new IllegalAccessException();
                            }
                        }
                    }

                    if (course.StudentsToAdd != null)
                    {
                        foreach (var student in course.StudentsToAdd)
                        {
                            editingCourse.Students.Add(new CourseEnrollment
                            {
                                CourseId = editingCourse.Id,
                                UserId = student
                            });
                        }
                    }
                    if (course.StudentsToRemove != null)
                    {
                        var studentsToRemove = course.StudentsToRemove
                            .Where(userId => editingCourse.Students.Any(student => student.UserId == userId))
                            .Select(studentId => editingCourse.Students.First(student => student.UserId == studentId));


                        foreach (var student in studentsToRemove)
                        {
                            editingCourse.Students.Remove(student);
                        }
                    }

                    _context.SaveChanges();

                    return GetCourse(callerId, courseId);
                }
                else
                {
                    throw new IllegalAccessException();
                }
            }
            else
            {
                throw new NotFoundException();
            }
        }

        public bool Delete(string userId, int courseId)
        {
            Course course = _context.Courses
                .Include(c => c.Creator)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                throw new NotFoundException();
            }
            if (!CanDeleteCourse(userId, courseId))
            {
                throw new IllegalAccessException();
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();

            return true;

        }

        public bool CanDeleteCourse(string userId, int courseId)
        {
            var adminRole = _context.Roles
                        .Include(role => role.Users)
                        .FirstOrDefault(role => role.Name == RoleNames.Admin);

            var user = _context.Users
                .Include(u => u.CreatedCourses)
                .FirstOrDefault(u => u.Id == userId);

            return (user != null && user.CreatedCourses.Any(c => c.Id == courseId))
                || (adminRole != null && adminRole.Users.Any(u => u.UserId == userId));
        }

    }
}
