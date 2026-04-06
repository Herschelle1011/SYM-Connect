using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rotativa.AspNetCore;


namespace SYM_CONNECT.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
       
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {


            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {

            var user = new Users
            {
                CreatedAt = DateTime.Now,
            };

         

            return View(user);
        }



        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("Id,FullName,Email,PasswordHash,Role,Status,CreatedAt")]
          Users users, bool generate = false)
        {
            //  Generate Password button 
            if (generate)
            {
                var generated = GeneratePassword();
                users.PasswordHash = generated;
                ViewBag.GeneratedPassword = generated;
                ModelState.Clear();
                return View(users);
            }

            // Password length check 
            if (string.IsNullOrEmpty(users.PasswordHash) ||
                users.PasswordHash.Length < 8)
            {
                ModelState.AddModelError("PasswordHash",
                    "Password must be at least 8 characters.");
                return View(users);
            }

            // Email already exists 
            bool exists = await _context.Users.AnyAsync(u => u.Email == users.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email already exists!");
                return View(users);
            }

            // STEP 5: Allowed email only
            var allowedDomains = new List<string>
    {
        "gmail.com", "yahoo.com", "outlook.com", "hotmail.com"
    };
            string domain = users.Email.Split('@')[1].ToLower();
            if (!allowedDomains.Contains(domain))
            {
                ModelState.AddModelError("Email", "Email domain is not allowed.");
                return View(users);
            }

            //  Email format check
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(users.Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Invalid email format.");
                return View(users);
            }

            // Double dot check 
            if (users.Email.Contains(".."))
            {
                ModelState.AddModelError("Email", "Email cannot contain consecutive dots.");
                return View(users);
            }

            //  final check then Save 
            if (ModelState.IsValid)
            {
                users.Status = "Active";         
                users.CreatedAt = DateTime.Now;  
                _context.Add(users);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User Added Successfully! {users.Email}";
                return RedirectToAction(nameof(Index));
            }

            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null) return NotFound();
            


            var Users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id); 


            if (Users == null)
            {
                return NotFound();
            }
            return View(Users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Role,Status,CreatedAt")] Users users)
        {

            if (id != users.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {             

                try
                {
                    var existing = await _context.Users.FindAsync(id);
                    if (existing == null) return NotFound();
                    existing.FullName = users.FullName;
                    existing.Email = users.Email;
                    existing.Status = users.Status;
                    existing.Role = users.Role;
                    existing.CreatedAt = users.CreatedAt;
                    TempData["Success"] = $"{users.Email} Updated Successfully!";

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

 

            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users != null)
            {
                _context.Users.Remove(users);
            }

            TempData["Error"] = $"{users.Email} Deleted!";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET
        public async Task<IActionResult> Inactive(int? id)
        {
            var USER = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (USER == null)
            {
                return NotFound();
            }
            return View(USER);
        }

        [HttpPost, ActionName("Inactive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactiveConfirmed(int? id)
        {
            var USER = await _context.Users.FirstAsync(u => u.Id == id);
            if (USER == null) {

                return NotFound();
            }

            USER.Status = "Inactive";
            USER.InactiveAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Warning"] = $"{USER.Email} Status to Inactive";
            return RedirectToAction(nameof(Index));
        }

        //RESTORE SECTION
        [HttpPost, ActionName("Restored")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restored(int? id)
        {
            var USER = await _context.Users.FirstAsync(u => u.Id == id);
            if (USER == null)
            {
                return NotFound();
            }

            USER.Status = "Active";
            USER.InactiveAt = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{USER.Email} Account Restored";
            return RedirectToAction(nameof(Index));
        }


        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }


        //  generates a strong password 
        private string GeneratePassword()
        {
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lower = "abcdefghijklmnopqrstuvwxyz";
            var numbers = "0123456789";
            var symbols = "@#$!%*?&";
            var all = upper + lower + numbers + symbols;

            var random = new Random();
            var password = new List<char>();

            // guarantee at least one of each type
            password.Add(upper[random.Next(upper.Length)]);
            password.Add(lower[random.Next(lower.Length)]);
            password.Add(numbers[random.Next(numbers.Length)]);
            password.Add(symbols[random.Next(symbols.Length)]);

            // fill remaining up to 12 characters
            for (int i = 4; i < 12; i++)
            {
                password.Add(all[random.Next(all.Length)]);
            }

            // shuffle so it is not predictable
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email, int? id)
        {
            var exists = await _context.Users
                                       .AnyAsync(u => u.Email == email
                                                   && u.Id != id); 
            if (exists)
            {
                return Json($"Email '{email}' is already registered.");
            }

            return Json(true);
        }

        private void PopulateViewBag()
        {
            var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.CurrentUser = currentUserEmail;
            ViewBag.CurrentUserId = currentUserId;

            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email");
        }


        public async Task<IActionResult> ExportPDF()
        {
            // Get ALL users from database
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Return as PDF using the ExportPDF view
            return new ViewAsPdf("ExportPDF", users)
            {
                FileName = $"Users_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore
                                    .Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10
                }
            };
        }



    }
}
