using Microsoft.AspNetCore.Mvc;
using MVCCollegeWeb.Databases;
using MVCCollegeWeb.Models;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MVCCollegeWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(IFormCollection form)
        {
            var login = LoginViewModel.GetInstance();

            await login.Authenticated(form["username"], form["password"]);

            if (!login.IsAuthenticated)
            {
                ModelState.AddModelError(string.Empty, "Désolé, votre nom d'utilisateur ou mot de passe est invalide!");
                return View("Login", login);
            }

            var index = IndexViewModel.GetInstance(_logger);

            await index.GetCourses();
            await index.GetStudents();

            return View("Index", index);
        }

        public IActionResult Login()
        {
            LoginViewModel.GetInstance().Logout();
            return View();
        }

        public IActionResult Logout()
        {
            LoginViewModel.GetInstance().Logout();
            return View();
        }

        public IActionResult CoursView()
        {
            var index = IndexViewModel.GetInstance(_logger);
            return PartialView("_CoursView", index);
        }

        public IActionResult StudentView()
        {
            var index = IndexViewModel.GetInstance(_logger);
            return PartialView("_StudentView", index);
        }

        public IActionResult CourseTable(int teacherId)
        {
            var index = IndexViewModel.GetInstance(_logger);
            index.UpdateCourseTeacher(teacherId).Wait();
            return PartialView("_CoursTable", index);
        }

        public IActionResult CoursCombinedView(string scheduleId)
        {
            var index = IndexViewModel.GetInstance(_logger);
            index.GetStudentsByCourse(scheduleId).Wait();

            return PartialView("_CoursCombinedView", index);
        }

        public IActionResult CallModalStudent()
        {
            var index = IndexViewModel.GetInstance(_logger);
            return PartialView("_ModalStudent", index);
        }

        public IActionResult GetAllCourses()
        {
            var model = IndexViewModel.GetInstance(_logger);

            model.GetCourses().Wait();

            return View(model);
        }

        public IActionResult UpdateGrade(int studentId, decimal grade)
        {
            try
            {
                var index = IndexViewModel.GetInstance(_logger);
                index.UpdateGrade(studentId, grade).Wait();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error from updating the grade.");

                return Json(new { success = false, message = "Error from updating the grade." });
            }
        }

        public IActionResult InsertCourseStudent(int studentId)
        {
            var index = IndexViewModel.GetInstance(_logger);
            index.InsertStudentCourse(studentId).Wait();
            return PartialView("_CoursCombinedView", index);
        }

        public IActionResult RemoveStudentFromCourse(int studentId)
        {
            try
            {
                var index = IndexViewModel.GetInstance(_logger);
                index.RemoveStudentCourse(studentId).Wait();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing student from course.");

                return Json(new { success = false, message = "Error removing student from course." });
            }
        }

        public IActionResult UpdateStudentInfo(Person student)
        {
            try
            {
                var index = IndexViewModel.GetInstance(_logger);
                index.UpdatePersonInfo(student).Wait();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student information.");

                return Json(new { success = false, message = "Error updating student information." });
            }
        }
        
        public IActionResult GetFilteredSemesters(string semester = "", string department = "", string course = "")
        {
            var index = IndexViewModel.GetInstance(_logger);

            if (!string.IsNullOrEmpty(semester))
            {
                index.FilterBySemester(semester);
            }

            if (!string.IsNullOrEmpty(department))
            {
                index.FilterByDepartment(department);
            }

            if (!string.IsNullOrEmpty(course))
            {
                index.FilterByCourse(course);
            }

            return PartialView("_CoursTable", index);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}