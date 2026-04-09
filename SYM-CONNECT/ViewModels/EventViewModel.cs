using SYM_CONNECT.Models;

namespace SYM_CONNECT.ViewModels
{
    public class EventViewModel
    {
        public List<Event> Events { get; set; } //get all events
        public List<Event> CancelledEvents { get; set; } //get all cancelled events separate

        // selected month and year 
        public int? SelectedMonth { get; set; } //CAN BE NULL FOR FILTERING ONLY
        public int? SelectedYear { get; set; }
    }
}
