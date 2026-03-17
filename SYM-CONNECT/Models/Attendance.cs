namespace SYM_CONNECT.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public int PointsEarned { get; set; }

        // Navigation properties
        public Users? User { get; set; }

        public Event? Event { get; set; }
    }
}
