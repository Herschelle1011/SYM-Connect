using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYM_CONNECT.Models
{
    public class Event
    {

        [Key]
        public int EventId { get; set; }  //unique  eventid

        [Required]
        public string Title { get; set; } = string.Empty;  //event title

        [Required]
        public string Description { get; set; } = string.Empty; //evemt description 

        [Required]
        public DateTime EventDate { get; set; } // event date
        public DateTime? EndDate { get; set; } // event end date

        [Required]
        public int CreatedBy { get; set; } // for database
        public Users? CreatedByUser { get; set; } //created by current user

        public string ApprovalStatus { get; set; } = "Pending"; // default to pending 
        public int? ApprovedBy { get; set; } //approved by specific id of the user
        public Users? ApprovedByUser { get; set; } // by who

        // cancellation section -----------
        public bool? IsCancelled { get; set; } = false; 

        [NotMapped] 
        public bool IsActuallyCancelled => CancelledAt != null; //IS IT  CANCELLED?
        public DateTime? CancelledAt { get; set; } //DATE OF WHEN IS IT CANCELLED

        public int? CancelledBy { get; set; }
        public Users? CancelledByUser { get; set; }

        public int? EventHandlerId { get; set; }

        [ForeignKey("EventHandlerId")]
        public Users? EventHandlerBy { get; set; }

        [NotMapped] //events  progress
        public string EventProgress
        {
            get
            {
               //GET EVENTPROGRESS BY ALSO THIS SELECTION CONDITION
                if (EventDate > DateTime.Now)
                    return "Upcoming";

                if (EndDate.HasValue && EndDate >= DateTime.Now)
                    return "Ongoing";

                return "Done";
            }
        }

        //GET ASSIGNED GROUPS LIST
        public ICollection<SYMGroup> AssignedGroups { get; set; } = new List<SYMGroup>();
    }

   //event status section -------
    public static class EventStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
