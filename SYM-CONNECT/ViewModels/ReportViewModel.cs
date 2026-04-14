namespace SYM_CONNECT.ViewModel
{
    //MAIN DASHBOARD REPORT VIEWMODEL
    //THIS IS USED FOR SUMMARY DASHBOARD (COUNTS, STATS, CHARTS, ETC.)
    public class ReportViewModel
    {
        public int TotalMembers { get; set; } //ALL MEMBERS IN SYSTEM
        public int ActiveMembers { get; set; } //MEMBERS WITH STATUS = ACTIVE
        public int InactiveMembers { get; set; } //MEMBERS WITH STATUS = INACTIVE

        public int TotalLeaders { get; set; } //TOTAL USERS WITH ROLE = LEADER
        public int TotalGroups { get; set; } //TOTAL NUMBER OF GROUPS

        public int TotalEvents { get; set; } //ALL EVENTS CREATED
        public int DoneEvents { get; set; } //COMPLETED / FINISHED EVENTS

        public int MaxMembers { get; set; } //MAX MEMBERS IN GROUP S
        public int? SelectedDay { get; set; }


        public int MemberDelta { get; set; } //DIFFERENCE BETWEEN CURRENT AND PREVIOUS MONTH
        public int PrevMonthMembers { get; set; } //MEMBER COUNT LAST MONTH 

        //  FILTERS FOR DROPDOWN PURPOSES /
        public int? SelectedMonth { get; set; } //SELECTED MONTH FILTER (NULL = ALL)
        public int? SelectedYear { get; set; } //SELECTED YEAR FILTER

        public List<SYM_CONNECT.Models.SYMGroup> Groups { get; set; } = new();
        //ALL GROUPS — USED FOR DISPLAY OR LOOPING IN DASHBOARD

        public List<LeaderRowViewModel> LeaderData { get; set; } = new();
        //CUSTOM STRUCTURE FOR LEADER + THEIR GROUP + MEMBER COUNT

        public List<SYM_CONNECT.Models.Event> RecentEvents { get; set; } = new();
        //RECENT EVENTS — USED FOR ACTIVITY FEED / LATEST EVENTS SECTION
        public List<SYM_CONNECT.Models.Event> GetAllEvents { get; set; } = new();


        public List<int> EventsPerMonth { get; set; } = new();
    }

    //LEADER ROW VIEWMODEL  USED FOR DISPLAYING LEADER + GROUP INFO TOGETHER
    public class LeaderRowViewModel
    {
        public SYM_CONNECT.Models.Users Leader { get; set; }

        public SYM_CONNECT.Models.SYMGroup? Group { get; set; }
        //GROUP ASSIGNED TO THIS LEADER (CAN BE NULL IF NO GROUP)

        public int? Members { get; set; }
        //NUMBER OF MEMBERS IN THAT LEADERS GROUP (NULL IF NO GROUP)

        public int? SelectedDay { get; set; }
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
    }
}