using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // ─── THIS SECTION HAS FOREIGN KEYS FROM OTHER MODEL CLASSES FOR REPORT PURPOSES IN THE DASHBOARD OVERVIEW
        private async Task<ReportViewModel> BuildDashboardViewModel(int? month = null, int? year = null)
        {
            //FILTERS SECTION
            int filterYear = year ?? DateTime.Now.Year;
            int filterMonth = month ?? DateTime.Now.Month;
            bool hasFilter = month.HasValue || year.HasValue;

            // All groups FROM THE SYMGROUP
            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();

            // ── Counts filtered by selected period
            // Members joined in the selected month+year
            var membersQuery = _db.Users.AsQueryable();
            if (year.HasValue) membersQuery = membersQuery.Where(u => u.CreatedAt.Year == year.Value);
            if (month.HasValue) membersQuery = membersQuery.Where(u => u.CreatedAt.Month == month.Value);

            var filteredMembers = await membersQuery.ToListAsync();

            // Previous period member count for delta (same month last year OR last month)
            DateTime prevDate = new DateTime(filterYear, filterMonth, 1).AddMonths(-1);
            int prevMonthMembers = await _db.Users
                .CountAsync(u => u.CreatedAt.Year == prevDate.Year
                              && u.CreatedAt.Month == prevDate.Month);

            // Leaders filtered by period WHO LEADERS QUERY BY SELECTED EVENT AND MONTH TO SHOW REPORT 
            var leadersQuery = _db.Users.Where(u => u.Role == "Leader");
            if (year.HasValue) leadersQuery = leadersQuery.Where(u => u.CreatedAt.Year == year.Value);
            if (month.HasValue) leadersQuery = leadersQuery.Where(u => u.CreatedAt.Month == month.Value);
            var filteredLeaders = await leadersQuery.ToListAsync();

            // Events filtered by period THE SAME IN THE ABOVE BUT FOR EVENTS
            var eventsQuery = _db.Events.AsQueryable();
            if (year.HasValue) eventsQuery = eventsQuery.Where(e => e.EventDate.Year == year.Value);
            if (month.HasValue) eventsQuery = eventsQuery.Where(e => e.EventDate.Month == month.Value);
            var filteredEvents = await eventsQuery.ToListAsync();

            // Recent events for the table GET RECENT EVENTS
            var recentEventsQuery = _db.Events
                .Include(e => e.AssignedGroups) //INCLUDE ASSIGNEDGROUPS
    .Where(e => e.IsCancelled == false && e.EventDate < DateTime.Now);
            if (year.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Year == year.Value);
            if (month.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Month == month.Value);
            var recentEvents = await recentEventsQuery
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            // Events per month for chart (full selected year)
            var allYearEvents = await _db.Events
                .Where(e => e.EventDate.Year == filterYear)
                .ToListAsync();

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1;

            // CALCULATIONS PURPOSES COUNTS FOR ACTIVE/INACTIVE USERS PER MONTH 
            int memberDelta = filteredMembers.Count - prevMonthMembers;
            int activeCount = hasFilter ? filteredMembers.Count(u => u.Status == "Active")
                                         : await _db.Users.CountAsync(u => u.Status == "Active");
            int inactiveCount = hasFilter ? filteredMembers.Count(u => u.Status == "Inactive")
                                          : await _db.Users.CountAsync(u => u.Status == "Inactive");

            return new ReportViewModel //TO REPORTVIEWMODEL
            {
                TotalMembers = hasFilter ? filteredMembers.Count
                                            : await _db.Users.CountAsync(), //COUNT TOTAL MEMBERS
                ActiveMembers = activeCount, //ACTIVE MEMBERS COUNT
                InactiveMembers = inactiveCount, //INACTIVE MEMBERS COUNT
                TotalLeaders = hasFilter ? filteredLeaders.Count
                                            : await _db.Users.CountAsync(u => u.Role == "Leader"), //COUNT USERS WHERE ROLE IS LEADER
                TotalGroups = await _db.SYMGroup.CountAsync(), //COUNT GROUP
                TotalEvents = filteredEvents.Count, //SELECTED TOTAL EVEMTS
                DoneEvents = filteredEvents.Count(e => e.EventDate < DateTime.Now), //ALREADY DONE ENVENTS
                 
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups, //GROUPS
                RecentEvents = recentEvents, //GET RECENT EVENTS

                EventsPerMonth = Enumerable.Range(1, 12)
                    .Select(m => allYearEvents.Count(e => e.EventDate.Month == m))
                    .ToList(),

                LeaderData = filteredLeaders.Any()
                    ? filteredLeaders.Select(l => new LeaderRowViewModel
                    {
                        Leader = l,
                        Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                        Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                    }).ToList()
                    : (await _db.Users.Where(u => u.Role == "Leader").ToListAsync())
                      .Select(l => new LeaderRowViewModel
                      {
                          Leader = l,
                          Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                          Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                      }).ToList(),

                SelectedMonth = month,
                SelectedYear = year,
                MemberDelta = memberDelta,
                PrevMonthMembers = prevMonthMembers
            };
        }

        // ─── Dashboard ────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")] //FOR VIEW PURPOSES WITH MONTH AND YEAR  FILTERS
        public async Task<IActionResult> Dashboard(int? month, int? year)
        {
            var vm = await BuildDashboardViewModel(month, year);
            return View(vm);
        }

        // ─── ExportPDF ────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportPDF()
        {
            var vm = await BuildDashboardViewModel();
            return new ViewAsPdf("ExportPDF", vm)
            {
                FileName = $"SYMConnect_Report_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins
                { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            };
        }

        // ─── UserManagement ───────────────────────────────────────────────────────
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
        public async Task<IActionResult> UserManagement(UserManagementViewModel viewModel) //USERMANAGEMENT SECTION
        {
            var model = viewModel.Form;

            if (!ModelState.IsValid)
            {
                viewModel.Users = await _db.Users.ToListAsync(); //GET ALL THE USERS RETURN
                return View(viewModel); 
            }

            bool exist = await _db.Users.AnyAsync(u => u.Email == model.Email); //DOES THE EMAIL ALREADY EXISTS?
            if (exist)
            {
                ModelState.AddModelError("Form.Email", "Email already exists!"); //SHOW VALIDATION MESSAGE
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            var allowedDomains = new List<string>
                { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com" }; //AVAILABLE ONLY DOMAINS FIXED!!
            string domain = model.Email.Split('@')[1].ToLower();
            if (!allowedDomains.Contains(domain))
            {
                ModelState.AddModelError("Form.Email", "Email domain is not allowed."); //SHOW VALIDATION IF NOT MATCHED
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(model.Email, emailPattern))
            {
                ModelState.AddModelError("Form.Email", "Invalid email format."); //CORRECT FORMAT CHECK
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

            if (model.Email.Contains("..")) //EMAIL DOESNT CONTAIN DOUBLE DOTS VALIDATION
            {
                ModelState.AddModelError("Form.Email", "Email cannot contain multiple dots.");
                viewModel.Users = _db.Users.ToList();
                return View(viewModel);
            }

 
            var user = new Users
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password,
                Role = model.Role,
                Status = model.Status,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();//SAVED TO DB
            return RedirectToAction("UserManagement");
        }

        // ─── Report ───────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Report()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        // ─── Error & Logout ───────────────────────────────────────────────────────
        public IActionResult Error()
        {
            return View(new ErrorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
