using Microsoft.AspNetCore.Mvc;
using SYM_CONNECT.Data;
using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; } 
        [Required]
        public string FullName { get; set; } //full name of the users

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
