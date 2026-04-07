namespace SYM_CONNECT.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; } //attendance id 

        public int UserId { get; set; } //for usersid for db

        public int EventId { get; set; } //for eventid for db

        public DateTime AttendanceDate { get; set; } //date

        public int PointsEarned { get; set; } //get points from specific event

        // Navigation properties
        public Users? User { get; set; }

        public Event? Event { get; set; }

        //public bool IsArchived { get; set; } = false;
    }
}
