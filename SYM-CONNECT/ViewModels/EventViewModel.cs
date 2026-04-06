using SYM_CONNECT.Models;

namespace SYM_CONNECT.ViewModels
{
    public class EventViewModel
    {
        public List<Event> Events { get; set; }
        public List<Event> CancelledEvents { get; set; }
    }
}
