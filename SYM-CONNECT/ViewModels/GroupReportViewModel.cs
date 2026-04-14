namespace SYM_CONNECT.ViewModel
{
    //MAIN VIEWMODEL FOR FULL GROUP REPORT PAGE
    //THIS HOLDS EVERYTHING NEEDED TO DISPLAY A COMPLETE GROUP REPORT
    public class GroupReportViewModel
    {
        // BASIC GROUP INFO 
        public int GroupId { get; set; } //UNIQUE GROUP ID
        public string GroupName { get; set; } = ""; //NAME OF THE GROUP
        public string Region { get; set; } = ""; //MAIN REGION
        public string SubRegion { get; set; } = ""; //SUB REGION
        public string Status { get; set; } = ""; //ACTIVE / INACTIVE
        public string LeaderName { get; set; } = "—"; //GROUP LEADER NAME (DEFAULT IF NONE)

        //  OVERVIEW STATS (TOP CARDS / SUMMARY SECTION) 
        public int TotalMembers { get; set; } //TOTAL MEMBERS INSIDE GROUP
        public int TotalEvents { get; set; } //TOTAL EVENTS JOINED / HANDLED
        public int TotalPoints { get; set; } //TOTAL POINTS EARNED BY GROUP
        public double AttendanceRate { get; set; }   //PERCENTAGE (0–100) HOW ACTIVE THEY ARE
        public int GroupRank { get; set; }   //POSITION OF THIS GROUP COMPARED TO OTHERS
        public int TotalGroupsCount { get; set; } //TOTAL NUMBER OF GROUPS (FOR RANKING CONTEXT)

        //  ACTIVITY TIMELINE (CHARTS / GRAPHS FOR LAST 12 MONTHS) 
        public List<int> MonthlyEventsAttended { get; set; } = new();  //12 VALUES (ONE PER MONTH)
        public List<int> MonthlyPointsEarned { get; set; } = new();    //12 VALUES FOR POINTS TREND

        //  MEMBER SUMMARY (TOP + WORST + INACTIVE MEMBERS SECTION) 
        public List<MemberSummaryRow> TopMembers { get; set; } = new(); //TOP 5 MEMBERS BASED ON POINTS
        public List<MemberSummaryRow> LeastActive { get; set; } = new(); //LOWEST ACTIVITY MEMBERS
        public List<MemberSummaryRow> NoAttendance { get; set; } = new(); //MEMBERS WITH ZERO EVENTS

        //  EVENT BREAKDOWN (TABLE OF EVENTS DETAILS)
        public List<EventBreakdownRow> EventBreakdown { get; set; } = new(); //LIST OF EVENTS + STATS

        //  BADGES / RECOGNITION ACHIEVEMENTS SECTION
        public List<string> BadgesEarned { get; set; } = new(); //LIST OF BADGE NAMES
        public List<MilestoneRow> Milestones { get; set; } = new(); //PROGRESS TRACKING ITEMS

        //  STAFF NOTES (ADMIN COMMENTS / INTERNAL NOTES SECTION) ───────────────
        public List<StaffNoteRow> StaffNotes { get; set; } = new(); //ALL EXISTING NOTES
        public string NewNote { get; set; } = "";  //INPUT FIELD FOR ADDING NEW NOTE

        //  LEADERBOARD (COMPARE THIS GROUP VS OTHER GROUPS) ────────────────────
        public List<GroupLeaderboardRow> Leaderboard { get; set; } = new(); //FULL RANKING TABLE

        //  FILTER SECTION (FOR DROPDOWN / YEAR FILTER) ─────────────────────────
        public int? SelectedYear { get; set; } //USED TO FILTER REPORT BY YEAR

        // In GroupReportViewModel and LeaderReportViewModel:
        public int? SelectedDay { get; set; }
        public int? SelectedMonth { get; set; }
 
    }

    //MEMBER SUMMARY ROW — USED IN MULTIPLE SECTIONS (TOP / LOW / ZERO MEMBERS)
    //THIS REPRESENTS ONE USER ROW WITH THEIR STATS
    public class MemberSummaryRow
    {
        public int UserId { get; set; } //USER ID
        public string FullName { get; set; } = ""; //USER FULL NAME
        public int EventsAttended { get; set; } //HOW MANY EVENTS THEY JOINED
        public int PointsEarned { get; set; } //TOTAL POINTS THEY EARNED
        public string Status { get; set; } = ""; //ACTIVE / INACTIVE STATUS
    }

    //EVENT BREAKDOWN ROW — USED FOR EVENT TABLE DISPLAY
    //EACH ROW = ONE EVENT WITH STATS
    public class EventBreakdownRow
    {
        public int EventId { get; set; } //EVENT ID
        public string Title { get; set; } = ""; //EVENT TITLE
        public DateTime EventDate { get; set; } //DATE OF EVENT
        public int Attendees { get; set; } //NUMBER OF PEOPLE WHO ATTENDED
        public int PointsAwarded { get; set; } //POINTS GIVEN IN THAT EVENT
        public string GroupName { get; set; } = ""; //WHICH GROUP THIS EVENT BELONGS TO
    }

    //MILESTONE ROW — USED FOR PROGRESS TRACKING (LIKE GOALS / TARGETS)
    public class MilestoneRow
    {
        public string Label { get; set; } = ""; //NAME OF MILESTONE (EX: "100 EVENTS")
        public int Target { get; set; } //TARGET VALUE
        public int Current { get; set; } //CURRENT PROGRESS

        //AUTO COMPUTED — CHECK IF TARGET IS REACHED
        public bool Achieved => Current >= Target;

        //AUTO COMPUTED — PERCENTAGE PROGRESS (0–100)
        public int Pct => Target > 0
            ? Math.Min(100, (int)((Current / (double)Target) * 100))
            : 0;
    }

    //STAFF NOTE ROW — USED FOR INTERNAL NOTES PER GROUP
    public class StaffNoteRow
    {
        public int Id { get; set; } //NOTE ID
        public string AuthorName { get; set; } = ""; //WHO CREATED THE NOTE
        public string Note { get; set; } = ""; //THE ACTUAL NOTE CONTENT
        public DateTime CreatedAt { get; set; } //WHEN IT WAS CREATED
    }

    //LEADERBOARD ROW — USED FOR COMPARING GROUPS
    //EACH ROW REPRESENTS ONE GROUP IN THE RANKING TABLE
    public class GroupLeaderboardRow
    {
        public int GroupId { get; set; } //GROUP ID
        public string GroupName { get; set; } = ""; //GROUP NAME
        public string LeaderName { get; set; } = "—"; //LEADER NAME

        public int TotalMembers { get; set; } //TOTAL MEMBERS IN GROUP
        public int TotalPoints { get; set; } //TOTAL POINTS
        public int TotalEvents { get; set; } //TOTAL EVENTS

        public double AttendanceRate { get; set; } //ACTIVITY RATE %

        public int MonthlyGrowth { get; set; }  //POINT GROWTH COMPARED TO LAST MONTH

        public bool IsCurrentGroup { get; set; } //TRUE IF THIS IS THE CURRENT VIEWED GROUP (FOR HIGHLIGHTING)
    }
}