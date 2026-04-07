using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SYM_CONNECT.Models
{
    public class SYMGroup
    {
        [Key]
        public int GroupId { get; set; } //groups id
        [Required]
        public string Name { get; set; } //name
        [Required]
        public string Region { get; set; } //its region
        [Required]
        public string SubRegion { get; set; } //subregion
        [Required]
        public string Status { get; set; } // Active / Inactive

        public int LeaderId { get; set; } //leaderId
        [ValidateNever]
        public Users? Leader { get; set; } //getItsLeader

        [ValidateNever]
        public ICollection<GroupMember> GroupMembers { get; set; } //list of groupsmem from groupmembers

        public ICollection<Event> Events { get; set; } = new List<Event>();   //list of events from Events
    }
}
