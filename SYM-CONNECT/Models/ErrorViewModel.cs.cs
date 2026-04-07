namespace SYM_CONNECT.Models
{
    public class ErrorViewModel
    {
        //shows errors or debugging
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
