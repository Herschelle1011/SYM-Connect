using Microsoft.AspNetCore.Mvc;

namespace SYM_CONNECT.ViewModels
{

      // ATTENDEE ROW — ONE RECORD PER MEMBER PER EVENT
    public class AttendeeRow
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;   // MEMBER FULL NAME
        public string Email { get; set; } = string.Empty;      // MEMBER EMAIL
        public string? GroupName { get; set; }                 // GROUP THEY BELONG TO
        public DateTime? CheckInTime { get; set; }             // NULL = ABSENT
        public int? PointsEarned { get; set; }
    }

    // MAIN VIEWMODEL FOR Attendees.cshtml
    public class AttendanceViewModel
    {
        // EVENT INFO
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;       // EVENT NAME SHOWN IN HEADER
        public DateTime EventStartTime { get; set; }                  // USED TO COMPUTE MINUTES LATE

        // GROUPS ASSIGNED TO THIS EVENT — FOR PILLS + DROPDOWN FILTER
        public List<string> AssignedGroups { get; set; } = new();

        // ATTENDANCE ROWS
        public List<AttendeeRow> Attendances { get; set; } = new();

        // SUMMARY COUNTS — SHOWN IN EVENT INFO STRIP
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }

        // FILTERS
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
    }

    
}
