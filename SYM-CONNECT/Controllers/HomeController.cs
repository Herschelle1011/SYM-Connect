using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
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
        public async Task<IActionResult> ExportPDF()
        {
            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();

            var leaders = await _db.Users
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            var allEvents = await _db.Events.ToListAsync();
            var currentYear = DateTime.Now.Year;

            var recentEvents = await _db.Events
                .Include(e => e.AssignedGroups)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1;

            var vm = new ReportViewModel
            {
                TotalMembers = await _db.Users.CountAsync(),
                ActiveMembers = await _db.Users.CountAsync(u => u.Status == "Active"),
                InactiveMembers = await _db.Users.CountAsync(u => u.Status == "Inactive"),
                TotalLeaders = await _db.Users.CountAsync(u => u.Role == "Leader"),
                TotalGroups = await _db.SYMGroup.CountAsync(),
                TotalEvents = await _db.Events.CountAsync(),
                UpcomingEvents = await _db.Events.CountAsync(e => e.EventDate > DateTime.Now),
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups,
                RecentEvents = recentEvents,
                EventsPerMonth = Enumerable.Range(1, 12)
                    .Select(m => allEvents.Count(e => e.EventDate.Year == currentYear
                                                   && e.EventDate.Month == m))
                    .ToList(),
                LeaderData = leaders.Select(l => new LeaderRowViewModel
                {
                    Leader = l,
                    Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                    Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                }).ToList()
            };

            return new ViewAsPdf("ExportPDF", vm)
            {
                FileName = $"SYMConnect_Report_{DateTime.Now:yyyyMMdd}.pdf",
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

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Dashboard()
        {
            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();

            var leaders = await _db.Users
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            var allEvents = await _db.Events.ToListAsync();
            var currentYear = DateTime.Now.Year;

            var recentEvents = await _db.Events
                .Include(e => e.AssignedGroups)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1;

            var vm = new ReportViewModel
            {
                TotalMembers = await _db.Users.CountAsync(),
                ActiveMembers = await _db.Users.CountAsync(u => u.Status == "Active"),
                InactiveMembers = await _db.Users.CountAsync(u => u.Status == "Inactive"),
                TotalLeaders = await _db.Users.CountAsync(u => u.Role == "Leader"),
                TotalGroups = await _db.SYMGroup.CountAsync(),
                TotalEvents = await _db.Events.CountAsync(),
                UpcomingEvents = await _db.Events.CountAsync(e => e.EventDate > DateTime.Now),
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups,
                RecentEvents = recentEvents,
                EventsPerMonth = Enumerable.Range(1, 12)
                    .Select(m => allEvents.Count(e => e.EventDate.Year == currentYear
                                                   && e.EventDate.Month == m))
                    .ToList(),
                LeaderData = leaders.Select(l => new LeaderRowViewModel
                {
                    Leader = l,
                    Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                    Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                }).ToList()
            };

            return View(vm);


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
            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();

            var leaders = await _db.Users
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            var allEvents = await _db.Events.ToListAsync();
            var currentYear = DateTime.Now.Year;

            var recentEvents = await _db.Events
                .Include(e => e.AssignedGroups)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1;

            var vm = new ReportViewModel
            {
                TotalMembers = await _db.Users.CountAsync(),
                ActiveMembers = await _db.Users.CountAsync(u => u.Status == "Active"),
                InactiveMembers = await _db.Users.CountAsync(u => u.Status == "Inactive"),
                TotalLeaders = await _db.Users.CountAsync(u => u.Role == "Leader"),
                TotalGroups = await _db.SYMGroup.CountAsync(),
                TotalEvents = await _db.Events.CountAsync(),
                UpcomingEvents = await _db.Events.CountAsync(e => e.EventDate > DateTime.Now),
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups,
                RecentEvents = recentEvents,
                EventsPerMonth = Enumerable.Range(1, 12)
                    .Select(m => allEvents.Count(e => e.EventDate.Year == currentYear
                                                   && e.EventDate.Month == m))
                    .ToList(),
                LeaderData = leaders.Select(l => new LeaderRowViewModel
                {
                    Leader = l,
                    Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                    Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                }).ToList()
            };

            return View(vm);
        }
    }
}
