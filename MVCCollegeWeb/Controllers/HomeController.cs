using Microsoft.AspNetCore.Mvc;
using MVCCollegeWeb.Models;
using System;
using System.Diagnostics;

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

            var index = IndexViewModel.GetInstance();

            await index.GetCourses();
            await index.GetPersons();

            return View("Index",index);
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
            var index = IndexViewModel.GetInstance();
            return PartialView("_CoursView", index);
        }

        public async Task<IActionResult> CoursCombinedView(string scheduleId)
        {
            var index = IndexViewModel.GetInstance();
            await index.GetStudentsByCourse(scheduleId);

            return PartialView("_CoursCombinedView", index);
        }

        public async Task<IActionResult> GetAllCourses()
        {
            var model = IndexViewModel.GetInstance();

            await model.GetCourses();

            return View(model);
        }

        [HttpGet]
        public IActionResult GetFilteredSemesters(string semester = "", string department = "", string course = "")
        {
            var index = IndexViewModel.GetInstance();  

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