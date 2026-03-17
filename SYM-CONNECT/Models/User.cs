using SYM_CONNECT.Data;
using System.ComponentModel.DataAnnotations;

namespace SYM_CONNECT.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
