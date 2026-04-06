using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace SYM_CONNECT.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db; //for database purposes

        public HomeController(AppDbContext db)
        {
            _db = db;
        }


        [Authorize(Roles = "admin")]
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult UserManagement()
        {



            var viewModel = new UserManagementViewModel
            {
                Users = _db.Users.ToList(),
                Form = new RegisterViewModel()
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserManagement(UserManagementViewModel viewModel)
        {
            var model = viewModel.Form; 

            if (!ModelState.IsValid)
            {
                viewModel.Users = await _db.Users.ToListAsync(); // reload users
                return View(viewModel);
            }

            bool exist = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (exist)
            {
                ModelState.AddModelError("Form.Email", "Email already exists!");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            var allowedDomains = new List<string>
    {
        "gmail.com", "yahoo.com", "outlook.com", "hotmail.com"
    };
            string domain = model.Email.Split('@')[1].ToLower();
            if (!allowedDomains.Contains(domain))
            {
                ModelState.AddModelError("Form.Email", "Email domain is not allowed.");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(model.Email, emailPattern))
            {
                ModelState.AddModelError("Form.Email", "Invalid email format.");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            if (model.Email.Contains(".."))
            {
                ModelState.AddModelError("Form.Email", "Email cannot contain multiple dots.");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("Form.Password", "Password doesn't match");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            var user = new Users
            {
                FullName = $"{model.FirstName} {model.LastName}", // ← fix FullName
                Email = model.Email,
                PasswordHash = model.Password,
                Role = model.Role,
                Status = model.Status,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("UserManagement");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel());
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Report()
        {
            ViewBag.TotalMembers = await _db.Users.CountAsync();
            ViewBag.ActiveMembers = await _db.Users.CountAsync(u => u.Status == "Active");
            ViewBag.TotalLeaders = await _db.Users.CountAsync(u => u.Role == "Leader");
            ViewBag.TotalGroups = await _db.SYMGroup.CountAsync();
            ViewBag.TotalEvents = await _db.Events.CountAsync();
            ViewBag.UpcomingEvents = await _db.Events.CountAsync(e => e.EventDate > DateTime.Now);

            return View();
        }
    }
}
