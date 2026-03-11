using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModel;

namespace SYM_CONNECT.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _db; //for database purposes

        public AccountController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult Login()
         {
            return View();
         }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _db.AppUsers.FirstOrDefault(u => u.Email == model.Email); //find email first 

            if (user == null || model.Password != user.PasswordHash)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if(user.Status == "Inactive")
            {
                ModelState.AddModelError(string.Empty, "Your account is Inactive, Contact Administrator!");
                return View();
            }

            HttpContext.Session.SetString("Id", user.Id.ToString());
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);


            //ERROR PART NO PASSWORD CONFIRMATION 

            // 6. Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Home"),
                "Leader" => RedirectToAction("Index", "Home"),
                "Member" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Login", "Account")
            };


        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

    }
}
