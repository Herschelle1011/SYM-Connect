using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Password doesn't match!")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}
