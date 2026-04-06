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
    ErrorMessage = "This email is already registered."
)]
        [Required]
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        //if set to  inactive
        public DateTime? InactiveAt { get; set; }

    }
}
