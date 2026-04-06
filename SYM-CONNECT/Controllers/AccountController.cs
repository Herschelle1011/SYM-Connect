using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModel;
using System.Security.Claims;

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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(model);
            }
            if (!ModelState.IsValid) return View(model);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("Password", "Invalid email or password.");
                    return View(model);
                }

                if (user.Status == "Inactive") //if status is inactive add error wont proceed
                {
                    ModelState.AddModelError("Password", "Your account is inactive. Contact Administrator!");
                    return View(model);
                }

            // VERIFY PASSWORD
            // COMPARE PLAIN PASSWORD
            if (user.PasswordHash != model.Password) // using PasswordHash as plain text for now
            {
                ModelState.AddModelError("Password", "Invalid email or password.");
                return View(model);
            }




            // CREATE SESSION & CLAIMS SAVED!
            HttpContext.Session.SetString("Id", user.Id.ToString());
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Email", user.Email);

                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // REDIRECT BASED ON ROLE
                return user.Role switch
                {
                    "admin" => RedirectToAction("Dashboard", "Home"),
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
