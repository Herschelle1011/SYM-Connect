using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required!")]
        [DataType(DataType.EmailAddress)] //for email
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [DataType(DataType.Password)] //for pass
        public string Password { get; set; }
    }
}
