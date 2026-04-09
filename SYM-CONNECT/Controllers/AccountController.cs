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
        [HttpGet]//GET    LOGIN  FOR VIEW
        public IActionResult Login()
         {
            return View();
         }

        [HttpPost]
        [ValidateAntiForgeryToken] //TOKEN  REQUIRED IF  LOGGED IN
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) //CHECK IF NOT  VALID
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(model);  //RETURN   TO  VIEW
            }
            if (!ModelState.IsValid) return View(model); 

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);  //GET  EMAIL  BY  INPUTTED EMAL  IF  EXISTED

                if (user == null)
                {
                    ModelState.AddModelError("Password", "Invalid email or password."); //INVALID IF SHOW NOTHING
                    return View(model);
                }

                if (user.Status == "Inactive") //if status is inactive add error wont proceed
                {
                    ModelState.AddModelError("Password", "Your account is inactive. Contact Administrator!"); //CANNOT  LOGIN IF INACTIVE
                    return View(model);
                }

            // VERIFY PASSWORD
            // COMPARE PLAIN PASSWORD
            if (user.PasswordHash != model.Password) // using PasswordHash as plain text for now  NOT  HASHED
            {
                ModelState.AddModelError("Password", "Invalid email or password.");
                return View(model);
            }




            // CREATE SESSION & CLAIMS SAVED!
            HttpContext.Session.SetString("Id", user.Id.ToString());   //FOR GETTING CURRENT  USER LOGIN / FULLNAME /  ROLE/ EMAIL
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

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);  //SIGNINSYNC TOKEN STORED

                // REDIRECT BASED ON ROLE 
                return user.Role switch
                {
                    "admin" => RedirectToAction("Dashboard", "Home"), //IF ADMIN
                    "Leader" => RedirectToAction("Index", "Home"), //IF LEADER
                    "Member" => RedirectToAction("Index", "Home"), //IF MEMBER
                    _ => RedirectToAction("Login", "Account") //IF NOT REDIRECT TO LOGIN
                };
            }


        [HttpGet] //GET  REGISTER
        public IActionResult Register()
        {
            return View(); //RETURN TO  VIEW
        }

    }
}
