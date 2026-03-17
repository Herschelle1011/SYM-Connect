using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SYM_CONNECT.Models
{
    public class SYMGroup
    {
        [Key]
        public int GroupId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string SubRegion { get; set; }
        [Required]
        public string Status { get; set; } // Active / Inactive

        public int LeaderId { get; set; }
        [ValidateNever]
        public Users? Leader { get; set; }

        [ValidateNever]
        public ICollection<GroupMember> GroupMembers { get; set; }
    }
}
