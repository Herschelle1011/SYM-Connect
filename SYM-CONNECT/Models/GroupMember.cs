using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SYM_CONNECT.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }

        public int GroupId { get; set; }
        [ValidateNever]
        public SYMGroup? Group { get; set; }

        public int UserId { get; set; }
        [ValidateNever]
        public Users? User { get; set; }
    }
}
