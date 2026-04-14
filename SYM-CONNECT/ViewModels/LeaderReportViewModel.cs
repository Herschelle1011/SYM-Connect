using SYM_CONNECT.ViewModel;

namespace SYM_CONNECT.ViewModels
{
    // LEADER REPORT VIEWMODEL — INDIVIDUAL LEADER PERFORMANCE REPORT
    public class LeaderReportViewModel
    {
        // ── LEADER INFO ──────────────────────────────────────────────────────
        public int LeaderId { get; set; }
        public string LeaderName { get; set; } = string.Empty;
        public string LeaderEmail { get; set; } = string.Empty;
        public string LeaderStatus { get; set; } = string.Empty;

        // ── GROUP INFO ───────────────────────────────────────────────────────
        public int? GroupId { get; set; }
        public string GroupName { get; set; } = "Unassigned";
        public string Region { get; set; } = "—";
        public string SubRegion { get; set; } = "—";
        public string GroupStatus { get; set; } = "No Group";

        // ── OVERVIEW STATS ───────────────────────────────────────────────────
        public int TotalMembers { get; set; }
        public int TotalEvents { get; set; }
        public int TotalPoints { get; set; }
        public double AttendanceRate { get; set; }
        public int LeaderRank { get; set; }
        public int TotalLeadersCount { get; set; }
        public int NoAttendanceCount { get; set; }
        public int CompletedEvents { get; set; }

        // ── CHART DATA ───────────────────────────────────────────────────────
        public List<int> MonthlyEventsAttended { get; set; } = new(); // 12 months
        public List<int> MonthlyPointsEarned { get; set; } = new();   // 12 months

        // ── MEMBER SUMMARIES ─────────────────────────────────────────────────
        public List<MemberSummaryRow> TopMembers { get; set; } = new();
        public List<MemberSummaryRow> LeastActive { get; set; } = new();
        public List<MemberSummaryRow> NoAttendance { get; set; } = new();

        // ── EVENT BREAKDOWN ──────────────────────────────────────────────────
        public List<EventBreakdownRow> EventBreakdown { get; set; } = new();

        // ── LEADERBOARD (all leaders ranked) ────────────────────────────────
        public List<LeaderLeaderboardRow> Leaderboard { get; set; } = new();

        // ── FILTER ───────────────────────────────────────────────────────────
        public int? SelectedYear { get; set; }

        // In GroupReportViewModel and LeaderReportViewModel:
        public int? SelectedDay { get; set; }
        public int? SelectedMonth { get; set; }

    }

    // ROW FOR LEADER LEADERBOARD TABLE
    public class LeaderLeaderboardRow
    {
        public int LeaderId { get; set; }
        public string LeaderName { get; set; } = string.Empty;
        public string GroupName { get; set; } = "Unassigned";
        public int TotalMembers { get; set; }
        public int TotalPoints { get; set; }
        public int TotalEvents { get; set; }
        public double AttendanceRate { get; set; }
        public bool IsCurrentLeader { get; set; }
    }
}