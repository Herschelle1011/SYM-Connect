using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModel;

namespace SYM_CONNECT.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        //FOR REPORT VIEW MODEL PURPOSES WITH FILTER MONTH AND YEAR
        private async Task<ReportViewModel> BuildReportViewModel(int? month = null, int? year = null)
        {
            int filterYear = year ?? DateTime.Now.Year; //ALLOW NULL YEAR
            int filterMonth = month ?? DateTime.Now.Month; //ALLOW NULL MONTH UNLESS SELECTED

            //GROUP GROUPS FROM SYMGROUP
            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();  

            var leaders = await _db.Users //GET LEADERS WHERE ROLE IS LEADERS FROM USER MODEL
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            var allEvents = await _db.Events.ToListAsync();  //GET  ALL EVENTS FROM THE EVENT MODEL TO LIST

            var recentEventsQuery = _db.Events //RECENT EVENTS  CHECK
                .Include(e => e.AssignedGroups)
                .Where(e => e.IsCancelled == false && e.EventDate < DateTime.Now);

            if (year.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Year == year.Value); //DOES IT HAVE VALUE?
            if (month.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Month == month.Value);  //DOES  IT  HAVE VALUE?

            var recentEvents = await recentEventsQuery
                .OrderByDescending(e => e.EventDate)
                .Take(10)
                .ToListAsync();  //TAKE 10 OF THE RECENT EVENTS  TO LIST 

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1; //GETMEMBERS   COUNT

            return new ReportViewModel
            {
                //FOR DISPLAY COUNTS OF EACH OF THIS REPORT VIEW MODEL PROPERTYS USING COUNTASYNC
                TotalMembers = await _db.Users.CountAsync(),
                ActiveMembers = await _db.Users.CountAsync(u => u.Status == "Active"),
                InactiveMembers = await _db.Users.CountAsync(u => u.Status == "Inactive"),
                TotalLeaders = await _db.Users.CountAsync(u => u.Role == "Leader"),
                TotalGroups = await _db.SYMGroup.CountAsync(),
                TotalEvents = await _db.Events.CountAsync(),
                DoneEvents = await _db.Events.CountAsync(e => e.EventDate < DateTime.Now),
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups,
                RecentEvents = recentEvents,
                SelectedMonth = month,
                SelectedYear = year,
                EventsPerMonth = Enumerable.Range(1, 12) //REPORT  FOR EVENTS PER MONTH
                    .Select(m => allEvents.Count(e => e.EventDate.Year == filterYear && e.EventDate.Month == m))
                    .ToList(),
                LeaderData = leaders.Select(l => new LeaderRowViewModel //FOR LEADERS   AND GROUP MEMBERS COUNT
                {
                    Leader = l,
                    Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                    Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                }).ToList()
            };
        }

        // ─── Index ────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var vm = await BuildReportViewModel(month, year);
            return View(vm);
        }

        // ─── ExportPDF ────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportPDF()
        {
            var vm = await BuildReportViewModel();
            return new ViewAsPdf("ExportPDF", vm)
            {
                FileName = $"SYMConnect_Report_{DateTime.Now:yyyyMMdd}.pdf", //FILE NAME FOR THE REPORT PDF
                PageSize = Rotativa.AspNetCore.Options.Size.A4, //SIZE OF THIS  PDF  
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            };
        }

        // ─── Group Report ─────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GroupReport(int? groupId, int? year)
        {
            int filterYear = year ?? DateTime.Now.Year;

            // Load all groups for leaderboard + selector
            var allGroups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .Include(g => g.Events)
                .ToListAsync();

            // Default to first group if none selected
            if (!groupId.HasValue && allGroups.Any())
                groupId = allGroups.First().GroupId;

            var group = allGroups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null) return NotFound();

            // ── Member stats from GroupMembers ───────────────────────────────
            var members = group.GroupMembers.ToList();

            // ── Events this group is assigned to in the filter year ───────────
            var groupEvents = await _db.Events
                .Include(e => e.AssignedGroups)
                .Where(e => e.AssignedGroups.Any(ag => ag.GroupId == groupId)
                         && e.EventDate.Year == filterYear
                         && e.IsCancelled == false)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            int totalPoints = members.Sum(m => m.TotalEarnedPoints);
            int totalMembers = members.Count;
            int totalEvents = groupEvents.Count;

            // Attendance rate = (sum of all attendees across events) / (members * events) * 100 //FORMULA
            // average fill per event vs total members
            double attendanceRate = totalMembers > 0 && totalEvents > 0
                ? Math.Round((double)groupEvents.Sum(e => e.AssignedGroups.Count) / (totalMembers * totalEvents) * 100, 1)
                : 0;  //ATTENDANCE RATE FORMULA ABOVE

            // Rank by total points among all groups (COMPARE)
            var rankedGroups = allGroups
                .OrderByDescending(g => g.GroupMembers.Sum(m => m.TotalEarnedPoints)) //TOTALEARNED OF THIS GROUPS
                .ToList();
            int rank = rankedGroups.FindIndex(g => g.GroupId == groupId) + 1;

           //GET MONTHLY EVENTS PURPOSES
            var monthlyEvents = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Count(e => e.EventDate.Month == m))
                .ToList();

            // Monthly points EARNED PER GROUP
            var monthlyPoints = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Where(e => e.EventDate.Month == m)
                                        .Sum(e => e.AssignedGroups.Count * 10)) // 10 pts per attendee placeholder
                .ToList();

            // ── Member summarieS  
            var memberRows = members.Select(m => new MemberSummaryRow
            {
                UserId = m.UserId,
                FullName = m.User?.FullName ?? "—",
                EventsAttended = groupEvents.Count(e => e.AssignedGroups.Any(ag => ag.GroupId == groupId)), // COUNT EVEMTS ATTENDED BY THE GROUP
                PointsEarned = m.TotalEarnedPoints, //EARNED POINTS
                Status = m.User?.Status ?? "—" 
            }).ToList();

            var topMembers = memberRows.OrderByDescending(m => m.PointsEarned).Take(5).ToList();
            var leastActive = memberRows.OrderBy(m => m.PointsEarned).Take(5).ToList();
            var noAttendance = memberRows.Where(m => m.PointsEarned == 0).ToList();

            // ── Event breakdown ───────────────────────────────────────────────
            var eventBreakdown = groupEvents.Select(e => new EventBreakdownRow
            {
                EventId = e.EventId,
                Title = e.Title,
                EventDate = e.EventDate,
                Attendees = e.AssignedGroups.Count,
                PointsAwarded = e.AssignedGroups.Count * 10, // placeholder
                GroupName = group.Name
            }).ToList();

            // ── BADGE SECTION PART
            var badges = new List<string>();
            if (totalEvents >= 10) badges.Add("10 Events Milestone");  //10 STREAK EVENTS
            if (totalPoints >= 500) badges.Add("500 Points Earner"); //IF THE GROUP HAS 500 POINTS EARNED THIS BADGE
            if (attendanceRate >= 90) badges.Add("Perfect Attendance"); //WOW PERFECT ATTENDANC BDAGE
            if (noAttendance.Count == 0) badges.Add("No One Left Behind");  //IF NO ATTENDANCE  COUNT = PERFECT ATTENDANCE  OF THIS MEMBERS
            if (rank == 1) badges.Add("Top Group"); //MOST RANKED GROUP
            if (totalMembers >= 10) badges.Add("Growing Community"); //IF GROUP HAS 10+ MEMBERS

            //FOR UI PURPOSES 
            var milestones = new List<MilestoneRow>
            {
                new() { Label = "Events attended",  Target = 10,  Current = totalEvents },
                new() { Label = "Points earned",    Target = 500, Current = totalPoints },
                new() { Label = "Members reached",  Target = 20,  Current = totalMembers },
                new() { Label = "Attendance rate",  Target = 100, Current = (int)attendanceRate },
            };

     

            // ── Leaderboard SETION PART
            var leaderboard = allGroups.Select(g =>
            {
                var gPoints = g.GroupMembers.Sum(m => m.TotalEarnedPoints);
                var gEvents = g.Events.Count(e => e.EventDate.Year == filterYear);
                var gRate = g.GroupMembers.Count > 0 && gEvents > 0
                    ? Math.Round((double)gEvents / (g.GroupMembers.Count * Math.Max(gEvents, 1)) * 100, 1) : 0;
                return new GroupLeaderboardRow
                {
                    GroupId = g.GroupId,
                    GroupName = g.Name,
                    LeaderName = g.Leader?.FullName ?? "—",
                    TotalMembers = g.GroupMembers.Count,
                    TotalPoints = gPoints,
                    TotalEvents = gEvents,
                    AttendanceRate = gRate,
                    MonthlyGrowth = gPoints / Math.Max(1, DateTime.Now.Month), // AVERAGE PER MONTH
                    IsCurrentGroup = g.GroupId == groupId
                };
            })
            .OrderByDescending(g => g.TotalPoints)
            .ToList();

            var vm = new GroupReportViewModel
            {
                //GROUP PURPOSES REPORTS
                GroupId = group.GroupId,
                GroupName = group.Name,
                Region = group.Region,
                SubRegion = group.SubRegion,
                Status = group.Status,
                LeaderName = group.Leader?.FullName ?? "—",
                TotalMembers = totalMembers,
                TotalEvents = totalEvents,
                TotalPoints = totalPoints,
                AttendanceRate = attendanceRate,
                GroupRank = rank,
                TotalGroupsCount = allGroups.Count,
                MonthlyEventsAttended = monthlyEvents,
                MonthlyPointsEarned = monthlyPoints,
                TopMembers = topMembers,
                LeastActive = leastActive,
                NoAttendance = noAttendance,
                EventBreakdown = eventBreakdown,
                BadgesEarned = badges,
                Milestones = milestones,
                Leaderboard = leaderboard,
                SelectedYear = year
            };

            // Pass group list for sidebar selector
            ViewBag.AllGroups = allGroups;
            ViewBag.SelectedYear = filterYear;

            return View("GroupReports", vm);
        }


    }
}