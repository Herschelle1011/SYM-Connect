using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SYM_CONNECT.Controllers
{
    public class SYMGroupsController : Controller
    {
        private readonly AppDbContext _context; //FOR DATABASE PURPOSES

        public SYMGroupsController(AppDbContext context)  //FOR DB PURPOSES
        {
            _context = context; //INITIALIZE DB
        }

        // GET: SYMGroups
        public async Task<IActionResult> Index()
        {
            //GET ALL GROUPS WITH LEADER AND MEMBERS
            var groups = await _context.SYMGroup
                .Include(s => s.Leader) //INCLUDE LEADER DATA
                .Include(s => s.GroupMembers) //INCLUDE MEMBERS
                .ToListAsync();

            //STORE INACTIVE GROUPS IN VIEWBAG FOR MODAL
            ViewBag.InactiveGroups = groups.Where(g => g.Status == "Inactive").ToList();

            //RETURN ONLY ACTIVE GROUPS IN MAIN VIEW
            return View(groups.Where(g => g.Status == "Active").ToList());
        }

        // GET: SYMGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); //IF NO ID

            //GET GROUP WITH LEADER AND MEMBERS + USER INFO
            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader) //GET LEADER
                .Include(s => s.GroupMembers)
                    .ThenInclude(m => m.User) //GET MEMBER USER DETAILS
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (sYMGroup == null) return NotFound(); //IF NOT FOUND

            return View(sYMGroup); //RETURN VIEW
        }

        // GET: SYMGroups/Create
        public IActionResult Create()
        {
            //GET ALL USERS WITH ROLE LEADER
            var ifLeader = _context.Users.Where(u => u.Role == "Leader");

            //SET DROPDOWN FOR LEADER
            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");

            return View(); //RETURN VIEW
        }

        // POST: SYMGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,Name,Region,SubRegion,Status,LeaderId")] SYMGroup sYMGroup)
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader"); //GET LEADERS AGAIN

            if (ModelState.IsValid) //CHECK IF VALID
            {
                _context.Add(sYMGroup); //ADD GROUP
                await _context.SaveChangesAsync(); //SAVE TO DB

                return RedirectToAction(nameof(Index)); //GO BACK TO LIST
            }

            //IF ERROR RELOAD DROPDOWN
            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");

            return View(sYMGroup); //RETURN WITH ERRORS
        }

        // GET: SYMGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader"); //GET LEADERS

            if (id == null) return NotFound(); //IF NO ID

            //GET GROUP WITH LEADER
            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (sYMGroup == null) return NotFound(); //IF NOT FOUND

            //SET DROPDOWN AGAIN
            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");

            return View(sYMGroup); //RETURN VIEW
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
       [Bind("GroupId,Name,Region,SubRegion,Status,LeaderId")] SYMGroup sYMGroup)
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader");

            if (id != sYMGroup.GroupId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.SYMGroup
                        .Include(g => g.GroupMembers)  // INCLUDE MEMBERS
                        .FirstOrDefaultAsync(g => g.GroupId == id);

                    if (existing == null) return NotFound();

                    // IF BEING SET TO INACTIVE — REMOVE ALL MEMBERS FIRST
                    if (sYMGroup.Status == "Inactive" && existing.Status == "Active")
                    {
                        var members = existing.GroupMembers.ToList();

                        if (members.Any())
                        {
                            _context.GroupMembers.RemoveRange(members);  // UNASSIGN ALL MEMBERS
                            await _context.SaveChangesAsync();           // SAVE REMOVAL FIRST
                        }

                        TempData["Success"] = $"'{existing.Name}' set to Inactive and {members.Count} member(s) unassigned.";
                    }
                    else
                    {
                        TempData["Success"] = $"'{existing.Name}' updated successfully.";
                    }

                    // UPDATE FIELDS
                    existing.Name = sYMGroup.Name;
                    existing.Region = sYMGroup.Region;
                    existing.SubRegion = sYMGroup.SubRegion;
                    existing.Status = sYMGroup.Status;
                    existing.LeaderId = sYMGroup.LeaderId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SYMGroupExists(sYMGroup.GroupId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");
            return View(sYMGroup);
        }

        [HttpGet]
        public async Task<IActionResult> GetOtherGroups(int currentGroupId)
        {
            //GET ALL GROUPS EXCEPT CURRENT + ONLY ACTIVE
            var groups = await _context.SYMGroup
                .Where(g => g.GroupId != currentGroupId && g.Status == "Active")
                .Select(g => new { g.GroupId, g.Name }) //RETURN ONLY NEEDED DATA
                .ToListAsync();

            return Json(groups); //RETURN AS JSON
        }

        // GET: SYMGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound(); //IF NO ID

            //GET GROUP WITH LEADER
            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (sYMGroup == null)
                return NotFound(); //IF NOT FOUND

            return View(sYMGroup); //RETURN VIEW
        }

        // POST: SYMGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sYMGroup = await _context.SYMGroup.FindAsync(id); //GET GROUP

            if (sYMGroup != null)
            {
                _context.SYMGroup.Remove(sYMGroup); //REMOVE GROUP
            }

            await _context.SaveChangesAsync(); //SAVE

            return RedirectToAction(nameof(Index)); //GO BACK
        }

        private bool SYMGroupExists(int id)
        {
            return _context.SYMGroup.Any(e => e.GroupId == id); //CHECK IF EXISTS
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableMembers()
        {
            //GET ALL USER IDS THAT ARE ALREADY ASSIGNED
            var assignedUserIds = await _context.GroupMembers
                .Select(gm => gm.UserId)
                .ToListAsync();

            //GET USERS WHO ARE NOT YET ASSIGNED
            var available = await _context.Users
                .Where(u => u.Role == "Member"
                         && !assignedUserIds.Contains(u.Id)) //NOT IN ANY GROUP
                .Select(u => new { u.Id, u.FullName, u.Email, u.Status })
                .ToListAsync();

            return Json(available); //RETURN JSON
        }

        // POST: ADD MEMBER TO GROUP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int userId)
        {
            //CHECK IF MEMBER ALREADY EXISTS IN GROUP
            bool alreadyExists = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

            if (alreadyExists)
            {
                TempData["Error"] = "Member already belongs to a group.";
                return RedirectToAction("Details", new { id = groupId });
            }

            //CREATE NEW MEMBER ENTRY
            var newMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                TotalEarnedPoints = 0
            };

            _context.GroupMembers.Add(newMember); //ADD
            await _context.SaveChangesAsync(); //SAVE

            TempData["Success"] = "Member added successfully!";

            return RedirectToAction("Details", new { id = groupId });
        }

        //TRANSFER MEMBER TO ANOTHER GROUP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferMember(int groupMemberId, int targetGroupId, int currentGroupId)
        {
            var member = await _context.GroupMembers.FindAsync(groupMemberId); //GET MEMBER

            if (member == null)
                return NotFound();

            //CHECK IF ALREADY IN TARGET GROUP
            bool alreadyInTarget = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == member.UserId && gm.GroupId == targetGroupId);

            if (alreadyInTarget)
            {
                TempData["Error"] = "Member is already in that group.";
                return RedirectToAction("Details", new { id = currentGroupId });
            }

            //TRANSFER MEMBER
            member.GroupId = targetGroupId;

            _context.GroupMembers.Update(member);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member transferred successfully!";

            return RedirectToAction("Details", new { id = currentGroupId });
        }

        //REACTIVATE GROUP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateGroup(int id)
        {
            var group = await _context.SYMGroup.FindAsync(id); //GET GROUP

            if (group == null) return NotFound();

            group.Status = "Active"; //SET ACTIVE
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{group.Name}' has been reactivated.";

            return RedirectToAction(nameof(Index));
        }

        //DELETE INACTIVE GROUP (HARD DELETE)
        [HttpPost, ActionName("DeleteInactiveGroup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInactiveGroupConfirmed(int id)
        {
            var group = await _context.SYMGroup.FindAsync(id); //GET GROUP

            if (group != null)
            {
                _context.SYMGroup.Remove(group); //REMOVE
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Group permanently deleted.";

            return RedirectToAction(nameof(Index));
        }

        //REMOVE MEMBER FROM GROUP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupMemberId)
        {
            var member = await _context.GroupMembers.FindAsync(groupMemberId); //GET MEMBER

            if (member == null)
                return NotFound();

            _context.GroupMembers.Remove(member); //REMOVE MEMBER
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member removed from group.";

            return RedirectToAction("Details", new { id = member.GroupId });
        }
    }
}