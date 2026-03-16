using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public string ApprovalStatus { get; set; } = "Pending";

        public int? ApprovedBy { get; set; }

        // Navigation properties
        public Users? CreatedByUser { get; set; }

        public Users? ApprovedByUser { get; set; }
    }
}