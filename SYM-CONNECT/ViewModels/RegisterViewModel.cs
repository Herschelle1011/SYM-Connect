using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is required")] //for email
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "FirstName is required")] //FIRSTNAME IF REQUIRED
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")] //LASTNAME IF REQUIRED
        public string LastName { get; set; } //lastname
        public string FullName => $"{FirstName} {LastName}";

        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required")] //pass is required
        public string Password { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Password doesn't match!")] //if dont match
        public string ConfirmPassword { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}
