using Microsoft.AspNetCore.Mvc;
using SYM_CONNECT.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYM_CONNECT.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; set; }

        [NotMapped] // won't create a column, just combines the two
        public string FullName => $"{FirstName} {LastName}";

        [Remote(
    action: "CheckEmail",
    controller: "Users",
    AdditionalFields = "Id",   // sends the Id along with email
    ErrorMessage = "This email is already registered." //automatically shows
)]
        [Required]
        public string Email { get; set; } //email
        public string? PasswordHash { get; set; } //password
        [Required]
        public string Role { get; set; } //role
        [Required]
        public string Status { get; set; } //get its status
        public DateTime CreatedAt { get; set; } //when created?

        //if set to  inactive
        public DateTime? InactiveAt { get; set; } //default to false if not inactive
                                                  // In Users.cs — add this:
        public ICollection<Attendance>? Attendances { get; set; } //for history attendances

    }
}
