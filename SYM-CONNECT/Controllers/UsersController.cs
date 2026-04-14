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
        private readonly AppDbContext _context; //FOR DATABASE PURPOSES

        public UsersController(AppDbContext context)
        {
            _context = context; //INITIALIZE DB
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            //GET ALL USERS
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); //IF NO ID
            }

            //GET USER BY ID
            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (users == null)
            {
                return NotFound(); //IF NOT FOUND
            }

            return View(users); //RETURN VIEW
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            //CREATE DEFAULT USER OBJECT
            var user = new Users
            {
                CreatedAt = DateTime.Now, //SET CREATED DATE
            };

            return View(user); //RETURN VIEW
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
[Bind("Id,FirstName,LastName,Email,PasswordHash,Role,Status,CreatedAt")]
Users users, bool generate = false)
        {
            //GENERATE PASSWORD BUTTON CLICKED
            if (generate)
            {
                var generated = GeneratePassword(); //GENERATE PASSWORD
                users.PasswordHash = generated; //SET PASSWORD
                ViewBag.GeneratedPassword = generated; //SHOW IN VIEW
                ModelState.Clear(); //CLEAR VALIDATION
                return View(users); //RETURN VIEW WITH GENERATED PASSWORD
            }

            //CHECK PASSWORD LENGTH
            if (string.IsNullOrEmpty(users.PasswordHash) ||
                users.PasswordHash.Length < 8)
            {
                ModelState.AddModelError("PasswordHash",
                    "Password must be at least 8 characters.");
                return View(users);
            }

            //CHECK IF EMAIL EXISTS
            bool exists = await _context.Users.AnyAsync(u => u.Email == users.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email already exists!");
                return View(users);
            }

            //CHECK ALLOWED EMAIL DOMAIN
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

            //CHECK EMAIL FORMAT
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";

            if (!Regex.IsMatch(users.Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Invalid email format.");
                return View(users);
            }

            //CHECK DOUBLE DOT ".."
            if (users.Email.Contains(".."))
            {
                ModelState.AddModelError("Email", "Email cannot contain consecutive dots.");
                return View(users);
            }

            //FINAL CHECK THEN SAVE
            if (ModelState.IsValid)
            {
                users.Status = "Active"; //DEFAULT ACTIVE
                users.CreatedAt = DateTime.Now; //SET CREATED DATE

                _context.Add(users); //ADD USER
                await _context.SaveChangesAsync(); //SAVE TO DB

                TempData["Success"] = $"User Added Successfully! {users.Email}";

                return RedirectToAction(nameof(Index)); //GO BACK
            }

            return View(users); //RETURN WITH ERRORS
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound(); //IF NO ID

            //GET USER
            var Users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Users == null)
            {
                return NotFound(); //IF NOT FOUND
            }

            return View(Users); //RETURN VIEW
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Role,Status,CreatedAt")] Users users)
        {
            if (id != users.Id)
            {
                return NotFound(); //ID MISMATCH
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Users.FindAsync(id); //GET EXISTING USER

                    if (existing == null) return NotFound();

                    //UPDATE FIELDS
                    existing.FirstName = users.FirstName;
                    existing.LastName = users.LastName;
                    existing.Email = users.Email;
                    existing.Status = users.Status;
                    existing.Role = users.Role;
                    existing.CreatedAt = users.CreatedAt;

                    TempData["Success"] = $"{users.Email} Updated Successfully!";

                    await _context.SaveChangesAsync(); //SAVE

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
                return NotFound(); //IF NO ID
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (users == null)
            {
                return NotFound(); //IF NOT FOUND
            }

            return View(users); //RETURN VIEW
        }

        // POST: DELETE USER
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id); //GET USER

            if (user != null)
            {
                //STEP 1: REMOVE EVENT HANDLER CONNECTIONS
                var assignedGroup = await _context.SYMGroup
                    .FirstOrDefaultAsync(g => g.LeaderId == id);

                var events = await _context.Events
                    .Where(e => e.EventHandlerId == id)
                    .ToListAsync();

                foreach (var ev in events)
                {
                    ev.EventHandlerId = null; //REMOVE HANDLER
                }

                await _context.SaveChangesAsync(); //SAVE FIRST

                //STEP 2: REMOVE LEADER FROM GROUP
                if (assignedGroup != null)
                {
                    assignedGroup.LeaderId = null;
                    _context.SYMGroup.Update(assignedGroup);
                    await _context.SaveChangesAsync();
                }

                //STEP 3: REMOVE GROUP MEMBERSHIPS
                var groupMemberships = await _context.GroupMembers
                    .Where(gm => gm.UserId == id)
                    .ToListAsync();

                if (groupMemberships.Any())
                {
                    _context.GroupMembers.RemoveRange(groupMemberships);
                    await _context.SaveChangesAsync();
                }

                //STEP 4: DELETE USER
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"{user?.Email} has been deleted.";

            return RedirectToAction(nameof(Index));
        }

        //GET INACTIVE VIEW
        public async Task<IActionResult> Inactive(int? id)
        {
            var USER = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (USER == null)
            {
                return NotFound(); //IF NOT FOUND
            }

            return View(USER);
        }

        //SET USER TO INACTIVE
        [HttpPost, ActionName("Inactive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactiveConfirmed(int? id)
        {
            var USER = await _context.Users.FirstAsync(u => u.Id == id);

            if (USER == null)
            {
                return NotFound();
            }

            USER.Status = "Inactive"; //SET STATUS
            USER.InactiveAt = DateTime.Now; //SET DATE

            await _context.SaveChangesAsync();

            TempData["Warning"] = $"{USER.Email} Status to Inactive";

            return RedirectToAction(nameof(Index));
        }

        //RESTORE USER ACCOUNT
        [HttpPost, ActionName("Restored")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restored(int? id)
        {
            var USER = await _context.Users.FirstAsync(u => u.Id == id);  //GETS USERS ID

            if (USER == null)
            {
                return NotFound();
            }

            USER.Status = "Active"; //SET BACK TO ACTIVE
            USER.InactiveAt = null; //REMOVE INACTIVE DATE

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{USER.Email} Account Restored";

            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id); //CHECK IF EXISTS
        }

        //GENERATE STRONG PASSWORD
        private string GeneratePassword()
        {
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lower = "abcdefghijklmnopqrstuvwxyz";
            var numbers = "0123456789";
            var symbols = "@#$!%*?&";
            var all = upper + lower + numbers + symbols;

            var random = new Random();
            var password = new List<char>();

            //ENSURE EACH TYPE EXISTS
            password.Add(upper[random.Next(upper.Length)]);
            password.Add(lower[random.Next(lower.Length)]);
            password.Add(numbers[random.Next(numbers.Length)]);
            password.Add(symbols[random.Next(symbols.Length)]);

            //FILL UNTIL 12 CHARACTERS
            for (int i = 4; i < 12; i++)
            {
                password.Add(all[random.Next(all.Length)]);
            }

            //SHUFFLE PASSWORD
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }

        //CHECK EMAIL DUPLICATE (AJAX)
        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email, int? id)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != id);

            if (exists)
            {
                return Json($"Email '{email}' is already registered.");
            }

            return Json(true); //VALID
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

        //EXPORT USERS TO PDF
        public async Task<IActionResult> ExportPDF()
        {
            //GET ALL USERS ORDERED
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            //RETURN PDF FILE
            return new ViewAsPdf("ExportPDF", users)
            {
                FileName = $"Users_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
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