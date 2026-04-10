using Microsoft.AspNetCore.Mvc;
using CCSMonitoringSystem.Models;
using CCSMonitoringSystem.Data;
using System.Linq;

namespace CCSMonitoringSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;

        public StudentController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Student/Index  →  Login page
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Student/Login
        [HttpPost]
        public IActionResult Login(Student loginData)
        {
            ModelState.Remove("LastName");
            ModelState.Remove("FirstName");
            ModelState.Remove("Email");
            ModelState.Remove("ConfirmPassword");

            // ADMIN LOGIN
            if (loginData.IdNumber == "admin" && loginData.Password == "123456")
            {
                TempData["Admin"] = "Administrator";
                return RedirectToAction("AdminDashboard");
            }

            var student = _db.Students
                .FirstOrDefault(s => s.IdNumber == loginData.IdNumber
                                  && s.Password == loginData.Password);

            if (student == null)
            {
                ViewBag.Error = "Invalid ID Number or Password.";
                return View("Index", loginData);
            }

            TempData["LoggedIn"] = student.FirstName + " " + student.LastName;
            return RedirectToAction("Dashboard");
        }

        // GET: /Student/Dashboard  (simple landing after login)
        public IActionResult Dashboard()
        {
            if (TempData["LoggedIn"] == null)
                return RedirectToAction("Index");

            ViewBag.StudentName = TempData["LoggedIn"];
            return View();
        }

        // GET: /Student/AdminDashboard
        public IActionResult AdminDashboard()
        {
            if (TempData["Admin"] == null)
                return RedirectToAction("Index");

            ViewBag.Admin = TempData["Admin"];
            return View();
        }

        // GET: /Student/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Student/Register
        [HttpPost]
        public IActionResult Register(Student student)
        {
            ModelState.Remove("RememberMe");

            if (!ModelState.IsValid)
                return View(student);

            // Check for duplicate ID Number
            bool exists = _db.Students.Any(s => s.IdNumber == student.IdNumber);
            if (exists)
            {
                ModelState.AddModelError("IdNumber", "This ID Number is already registered.");
                return View(student);
            }

            _db.Students.Add(student);
            _db.SaveChanges();

            TempData["Success"] = "Registration successful! You can now log in.";
            return RedirectToAction("Index");
        }

        // GET: /Student/GetAllStudents - API endpoint for loading all students
        [HttpGet]
        public IActionResult GetAllStudents()
        {
            try
            {
                var students = _db.Students.ToList();
                return Json(students);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: /Student/SearchStudent - API endpoint for searching students
        [HttpGet]
        public IActionResult SearchStudent(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new List<Student>());
                }

                var students = _db.Students
                    .Where(s => s.IdNumber.Contains(query) || 
                               s.FirstName.Contains(query) || 
                               s.LastName.Contains(query))
                    .ToList();
                return Json(students);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: /Student/GetStudentById - API endpoint for getting a single student
        [HttpGet]
        public IActionResult GetStudentById(string id)
        {
            try
            {
                var student = _db.Students.FirstOrDefault(s => s.IdNumber == id);
                return Json(student);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: /Student/CreateSitIn - API endpoint for creating sit-in record
        [HttpPost]
        public IActionResult CreateSitIn([FromBody] SitInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdNumber))
                {
                    return Json(new { success = false, message = "ID Number is required" });
                }

                var student = _db.Students.FirstOrDefault(s => s.IdNumber == request.IdNumber);
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" });
                }

                // Here you would save the sit-in record to a SitIn table if you have one
                return Json(new { success = true, message = "Sit-In created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // Helper class for sit-in request
    public class SitInRequest
    {
        public string IdNumber { get; set; }
        public string Purpose { get; set; }
        public string Lab { get; set; }
    }
}
