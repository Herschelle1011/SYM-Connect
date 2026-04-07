using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SYM_CONNECT.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; } //groupmember  id

        public int GroupId { get; set; }  //group id
        [ValidateNever]
        public SYMGroup? Group { get; set; } //does it have group?

        public int UserId { get; set; } //get usersid
        [ValidateNever]
        public Users? User { get; set; } //freogin key with user

        public int TotalEarnedPoints { get; set; }  //overall points of the specific member
    }
}
