namespace SYM_CONNECT.ViewModel
{
    public class ReportViewModel
    {
        public int TotalMembers { get; set; } //get total overall for database
        public int ActiveMembers { get; set; }
        public int InactiveMembers { get; set; }
        public int TotalLeaders { get; set; }
        public int TotalGroups { get; set; }
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int MaxMembers { get; set; }

        public List<SYM_CONNECT.Models.SYMGroup> Groups { get; set; } = new(); //get groups
        public List<LeaderRowViewModel> LeaderData { get; set; } = new(); //get leaders
        public List<SYM_CONNECT.Models.Event> RecentEvents { get; set; } = new(); //get events 
        public List<int> EventsPerMonth { get; set; } = new(); //list permonths
    }

    public class LeaderRowViewModel
    {
        public SYM_CONNECT.Models.Users Leader { get; set; }
        public SYM_CONNECT.Models.SYMGroup? Group { get; set; }
        public int? Members { get; set; }
    }
}