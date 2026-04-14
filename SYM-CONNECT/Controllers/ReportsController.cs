using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModel;
using SYM_CONNECT.ViewModels;

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

        // ─── BUILD REPORT VIEW MODEL ──────────────────────────────────────────
        private async Task<ReportViewModel> BuildReportViewModel(int? month = null, int? year = null, int? day = null)
        {
            int filterYear = year ?? DateTime.Now.Year;
            bool hasFilter = day.HasValue || month.HasValue || year.HasValue;
            int totalGroups = await _db.SYMGroup.CountAsync();


            var groups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                .ToListAsync();

            var leaders = await _db.Users
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            var allEvents = await _db.Events.ToListAsync();

            // ── FILTERED USERS QUERY (by CreatedAt) ──────────────────────────────
            var usersQuery = _db.Users.AsQueryable();
            if (year.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAt.Year == year.Value);
            if (month.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAt.Month == month.Value);
            if (day.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAt.Day == day.Value);

     

            // ── FILTERED EVENTS QUERY (by EventDate) ─────────────────────────────
            var filteredEventsQuery = _db.Events.AsQueryable();
            if (year.HasValue) filteredEventsQuery = filteredEventsQuery.Where(e => e.EventDate.Year == year.Value);
            if (month.HasValue) filteredEventsQuery = filteredEventsQuery.Where(e => e.EventDate.Month == month.Value);
            if (day.HasValue) filteredEventsQuery = filteredEventsQuery.Where(e => e.EventDate.Day == day.Value);
            var filteredEvents = await filteredEventsQuery.ToListAsync();

            // ── RECENT EVENTS (same filter) ───────────────────────────────────────
            var recentEventsQuery = _db.Events
                .Include(e => e.AssignedGroups)
    .Where(e => e.IsCancelled == false && e.EventDate < DateTime.Now);
            if (year.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Year == year.Value);
            if (month.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Month == month.Value);
            if (day.HasValue) recentEventsQuery = recentEventsQuery.Where(e => e.EventDate.Day == day.Value);

            var recentEvents = await recentEventsQuery
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            int maxMembers = groups.Any() ? groups.Max(g => g.GroupMembers.Count) : 1;

            // COUNT use filtered when filter is active, total when no filter ──
            int totalMembers = hasFilter ? await usersQuery.CountAsync() : await _db.Users.CountAsync();
            int activeMembers = hasFilter ? await usersQuery.CountAsync(u => u.Status == "Active") : await _db.Users.CountAsync(u => u.Status == "Active");
            int inactiveMembers = hasFilter ? await usersQuery.CountAsync(u => u.Status == "Inactive") : await _db.Users.CountAsync(u => u.Status == "Inactive");
            int totalLeaders = hasFilter ? await usersQuery.CountAsync(u => u.Role == "Leader") : await _db.Users.CountAsync(u => u.Role == "Leader");


            // ── FILTERED LEADER DATA ──────────────────────────────────────────────
            var filteredLeaders = hasFilter
                ? await usersQuery.Where(u => u.Role == "Leader").ToListAsync()
                : leaders;

            return new ReportViewModel
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                InactiveMembers = inactiveMembers,
                TotalLeaders = totalLeaders,
                TotalEvents = filteredEvents.Count,
                DoneEvents = filteredEvents.Count(e => e.EventDate < DateTime.Now),
                MaxMembers = maxMembers == 0 ? 1 : maxMembers,
                Groups = groups,
                RecentEvents = recentEvents,
                SelectedMonth = month,
                TotalGroups = totalGroups,
                SelectedYear = year,
                SelectedDay = day,
                EventsPerMonth = Enumerable.Range(1, 12)
                    .Select(m => allEvents.Count(e => e.EventDate.Year == filterYear && e.EventDate.Month == m))
                    .ToList(),
                LeaderData = filteredLeaders.Select(l => new LeaderRowViewModel
                {
                    Leader = l,
                    Group = groups.FirstOrDefault(g => g.LeaderId == l.Id),
                    Members = groups.FirstOrDefault(g => g.LeaderId == l.Id)?.GroupMembers.Count
                }).ToList()
            };
        }
        // ─── Index ────────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int? month, int? year, int? day)
        {
            var vm = await BuildReportViewModel(month, year, day);
            return View(vm);
        }

        // ─── ExportPDF ────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportPDF()
        {
            var vm = await BuildReportViewModel();
            return new ViewAsPdf("ExportPDF", vm)
            {
                FileName = $"SYMConnect_Report_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            };
        }

        // ─── Group Report ─────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GroupReport(int? groupId, int? year, int? month, int? day)
        {
            int filterYear = year ?? DateTime.Now.Year;

            var allGroups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .Include(g => g.Events)
                .ToListAsync();

            if (!groupId.HasValue && allGroups.Any())
                groupId = allGroups.First().GroupId;

            var group = allGroups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null) return NotFound();

            var members = group.GroupMembers.ToList();

            // ── FILTERED group events (year + month + day) ────────────────────
            var groupEventsQuery = _db.Events
                .Include(e => e.AssignedGroups)
                .Where(e => e.AssignedGroups.Any(ag => ag.GroupId == groupId)
                         && e.EventDate.Year == filterYear
                         && e.IsCancelled == false);

            if (month.HasValue) groupEventsQuery = groupEventsQuery.Where(e => e.EventDate.Month == month.Value);
            if (day.HasValue) groupEventsQuery = groupEventsQuery.Where(e => e.EventDate.Day == day.Value);

            var groupEvents = await groupEventsQuery
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            int totalPoints = members.Sum(m => m.TotalEarnedPoints);
            int totalMembers = members.Count;
            int totalEvents = groupEvents.Count;

            double attendanceRate = totalMembers > 0 && totalEvents > 0
                ? Math.Round((double)groupEvents.Sum(e => e.AssignedGroups.Count) / (totalMembers * totalEvents) * 100, 1)
                : 0;

            var rankedGroups = allGroups
                .OrderByDescending(g => g.GroupMembers.Sum(m => m.TotalEarnedPoints))
                .ToList();
            int rank = rankedGroups.FindIndex(g => g.GroupId == groupId) + 1;

            var monthlyEvents = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Count(e => e.EventDate.Month == m))
                .ToList();

            var monthlyPoints = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Where(e => e.EventDate.Month == m)
                                        .Sum(e => e.AssignedGroups.Count * 10))
                .ToList();

            var memberRows = members.Select(m => new MemberSummaryRow
            {
                UserId = m.UserId,
                FullName = m.User?.FullName ?? "—",
                EventsAttended = groupEvents.Count(e => e.AssignedGroups.Any(ag => ag.GroupId == groupId)),
                PointsEarned = m.TotalEarnedPoints,
                Status = m.User?.Status ?? "—"
            }).ToList();

            var topMembers = memberRows.OrderByDescending(m => m.PointsEarned).Take(5).ToList();
            var leastActive = memberRows.OrderBy(m => m.PointsEarned).Take(5).ToList();
            var noAttendance = memberRows.Where(m => m.PointsEarned == 0).ToList();

            var eventBreakdown = groupEvents.Select(e => new EventBreakdownRow
            {
                EventId = e.EventId,
                Title = e.Title,
                EventDate = e.EventDate,
                Attendees = e.AssignedGroups.Count,
                PointsAwarded = e.AssignedGroups.Count * 10,
                GroupName = group.Name
            }).ToList();

            var badges = new List<string>();
            if (totalEvents >= 10) badges.Add("10 Events Milestone");
            if (totalPoints >= 500) badges.Add("500 Points Earner");
            if (attendanceRate >= 90) badges.Add("Perfect Attendance");
            if (noAttendance.Count == 0) badges.Add("No One Left Behind");
            if (rank == 1) badges.Add("Top Group");
            if (totalMembers >= 10) badges.Add("Growing Community");

            var milestones = new List<MilestoneRow>
            {
                new() { Label = "Events attended", Target = 10,  Current = totalEvents  },
                new() { Label = "Points earned",   Target = 500, Current = totalPoints  },
                new() { Label = "Members reached", Target = 20,  Current = totalMembers },
                new() { Label = "Attendance rate", Target = 100, Current = (int)attendanceRate },
            };

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
                    MonthlyGrowth = gPoints / Math.Max(1, DateTime.Now.Month),
                    IsCurrentGroup = g.GroupId == groupId
                };
            })
            .OrderByDescending(g => g.TotalPoints)
            .ToList();

            var vm = new GroupReportViewModel
            {
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
                SelectedYear = year,
                SelectedMonth = month,
                SelectedDay = day
            };

            ViewBag.AllGroups = allGroups;
            ViewBag.SelectedYear = filterYear;

            return View("GroupReports", vm);
        }

        // ─── Events Report ────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EventsReport(int? month, int? year, int? day)
        {
            var vm = await BuildReportViewModel(month, year, day);
            return View("EventsReports", vm);
        }

        // ─── Attendees ────────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Attendees(int eventId, int? month, int? year)
        {
            var ev = await _db.Events
                .Include(e => e.AssignedGroups)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null) return NotFound();

            var groupMembers = await _db.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .Where(gm => _db.Events
                    .Where(e => e.EventId == eventId)
                    .SelectMany(e => e.AssignedGroups)
                    .Any(g => g.GroupId == gm.GroupId))
                .ToListAsync();

            var attendanceRecords = await _db.Attendances
                .Where(a => a.EventId == eventId)
                .ToListAsync();

            if (month.HasValue)
                attendanceRecords = attendanceRecords.Where(a => a.AttendanceDate.Month == month.Value).ToList();
            if (year.HasValue)
                attendanceRecords = attendanceRecords.Where(a => a.AttendanceDate.Year == year.Value).ToList();

            var attendeeRows = groupMembers.Select(gm =>
            {
                var attendance = attendanceRecords.FirstOrDefault(a => a.UserId == gm.UserId);
                return new AttendeeRow
                {
                    UserId = gm.UserId,
                    FullName = gm.User?.FullName ?? "—",
                    Email = gm.User?.Email ?? "—",
                    GroupName = gm.Group?.Name ?? "—",
                    CheckInTime = attendance?.AttendanceDate,
                    PointsEarned = attendance?.PointsEarned ?? 0
                };
            }).ToList();

            int onTimeCount = 0, lateCount = 0, absentCount = 0;
            foreach (var row in attendeeRows)
            {
                if (row.CheckInTime == null) { absentCount++; }
                else
                {
                    var diff = (row.CheckInTime.Value - ev.EventDate).TotalMinutes;
                    if (diff > 0) lateCount++; else onTimeCount++;
                }
            }

            var vm = new AttendanceViewModel
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                EventStartTime = ev.EventDate,
                AssignedGroups = ev.AssignedGroups.Select(g => g.Name).Distinct().ToList(),
                Attendances = attendeeRows,
                OnTimeCount = onTimeCount,
                LateCount = lateCount,
                AbsentCount = absentCount,
                SelectedMonth = month,
                SelectedYear = year
            };

            return View("Attendees", vm);
        }

        // ─── Leader Report ────────────────────────────────────────────────────
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> LeaderReport(int? leaderId, int? year, int? month, int? day)
        {
            int filterYear = year ?? DateTime.Now.Year;

            var allLeaders = await _db.Users
                .Where(u => u.Role == "Leader")
                .ToListAsync();

            if (!leaderId.HasValue && allLeaders.Any())
                leaderId = allLeaders.First().Id;

            var leader = allLeaders.FirstOrDefault(u => u.Id == leaderId);
            if (leader == null) return NotFound();

            var allGroups = await _db.SYMGroup
                .Include(g => g.Leader)
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .Include(g => g.Events)
                .ToListAsync();

            var group = allGroups.FirstOrDefault(g => g.LeaderId == leaderId);

            List<SYM_CONNECT.Models.Event> groupEvents = new();
            List<GroupMember> members = new();

            if (group != null)
            {
                members = group.GroupMembers.ToList();

                // ── FILTERED leader events (year + month + day) ───────────────
                var groupEventsQuery = _db.Events
                    .Include(e => e.AssignedGroups)
                    .Where(e => e.AssignedGroups.Any(ag => ag.GroupId == group.GroupId)
                             && e.EventDate.Year == filterYear
                             && e.IsCancelled == false);

                if (month.HasValue) groupEventsQuery = groupEventsQuery.Where(e => e.EventDate.Month == month.Value);
                if (day.HasValue) groupEventsQuery = groupEventsQuery.Where(e => e.EventDate.Day == day.Value);

                groupEvents = await groupEventsQuery
                    .OrderByDescending(e => e.EventDate)
                    .ToListAsync();
            }

            int totalPoints = members.Sum(m => m.TotalEarnedPoints);
            int totalMembers = members.Count;
            int totalEvents = groupEvents.Count;
            int completedEvents = groupEvents.Count(e => e.EventDate < DateTime.Now);

            double attendanceRate = totalMembers > 0 && totalEvents > 0
                ? Math.Round((double)groupEvents.Sum(e => e.AssignedGroups.Count) / (totalMembers * totalEvents) * 100, 1)
                : 0;

            var rankedLeaders = allLeaders
                .Select(l => new
                {
                    Leader = l,
                    Points = allGroups.FirstOrDefault(g => g.LeaderId == l.Id)
                                      ?.GroupMembers.Sum(m => m.TotalEarnedPoints) ?? 0
                })
                .OrderByDescending(x => x.Points)
                .ToList();
            int rank = rankedLeaders.FindIndex(x => x.Leader.Id == leaderId) + 1;

            var monthlyEvents = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Count(e => e.EventDate.Month == m))
                .ToList();

            var monthlyPoints = Enumerable.Range(1, 12)
                .Select(m => groupEvents.Where(e => e.EventDate.Month == m)
                                        .Sum(e => e.AssignedGroups.Count * 10))
                .ToList();

            var memberRows = members.Select(m => new MemberSummaryRow
            {
                UserId = m.UserId,
                FullName = m.User?.FullName ?? "—",
                EventsAttended = groupEvents.Count(e => e.AssignedGroups.Any(ag => ag.GroupId == group!.GroupId)),
                PointsEarned = m.TotalEarnedPoints,
                Status = m.User?.Status ?? "—"
            }).ToList();

            var topMembers = memberRows.OrderByDescending(m => m.PointsEarned).Take(5).ToList();
            var leastActive = memberRows.OrderBy(m => m.PointsEarned).Take(5).ToList();
            var noAttendance = memberRows.Where(m => m.PointsEarned == 0).ToList();

            var eventBreakdown = groupEvents.Select(e => new EventBreakdownRow
            {
                EventId = e.EventId,
                Title = e.Title,
                EventDate = e.EventDate,
                Attendees = e.AssignedGroups.Count,
                PointsAwarded = e.AssignedGroups.Count * 10,
                GroupName = group?.Name ?? "—"
            }).ToList();

            var leaderboard = allLeaders.Select(l =>
            {
                var lGroup = allGroups.FirstOrDefault(g => g.LeaderId == l.Id);
                var lPoints = lGroup?.GroupMembers.Sum(m => m.TotalEarnedPoints) ?? 0;
                var lMembers = lGroup?.GroupMembers.Count ?? 0;
                var lEvents = lGroup?.Events.Count(e => e.EventDate.Year == filterYear) ?? 0;
                var lRate = lMembers > 0 && lEvents > 0
                    ? Math.Round((double)lEvents / (lMembers * Math.Max(lEvents, 1)) * 100, 1) : 0;

                return new LeaderLeaderboardRow
                {
                    LeaderId = l.Id,
                    LeaderName = l.FullName ?? "—",
                    GroupName = lGroup?.Name ?? "Unassigned",
                    TotalMembers = lMembers,
                    TotalPoints = lPoints,
                    TotalEvents = lEvents,
                    AttendanceRate = lRate,
                    IsCurrentLeader = l.Id == leaderId
                };
            })
            .OrderByDescending(l => l.TotalPoints)
            .ToList();

            var vm = new LeaderReportViewModel
            {
                LeaderId = leader.Id,
                LeaderName = leader.FullName ?? "—",
                LeaderEmail = leader.Email ?? "—",
                LeaderStatus = leader.Status ?? "—",
                GroupId = group?.GroupId,
                GroupName = group?.Name ?? "Unassigned",
                Region = group?.Region ?? "—",
                SubRegion = group?.SubRegion ?? "—",
                GroupStatus = group?.Status ?? "No Group",
                TotalMembers = totalMembers,
                TotalEvents = totalEvents,
                CompletedEvents = completedEvents,
                TotalPoints = totalPoints,
                AttendanceRate = attendanceRate,
                LeaderRank = rank,
                TotalLeadersCount = allLeaders.Count,
                NoAttendanceCount = noAttendance.Count,
                MonthlyEventsAttended = monthlyEvents,
                MonthlyPointsEarned = monthlyPoints,
                TopMembers = topMembers,
                LeastActive = leastActive,
                NoAttendance = noAttendance,
                EventBreakdown = eventBreakdown,
                Leaderboard = leaderboard,
                SelectedYear = year,
                SelectedMonth = month,
                SelectedDay = day
            };

            ViewBag.AllLeaders = allLeaders;
            ViewBag.SelectedYear = filterYear;

            return View("LeaderReports", vm);
        }
    }
}