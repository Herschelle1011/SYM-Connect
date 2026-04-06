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
        private readonly AppDbContext _context;

        public SYMGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SYMGroups
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SYMGroup.Include(s => s.Leader);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SYMGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader)
                .Include(s => s.GroupMembers)
                  .ThenInclude(m => m.User)         // load members
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (sYMGroup == null) return NotFound();

            return View(sYMGroup);
        }

        // GET: SYMGroups/Create
        public IActionResult Create()
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader");

            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");
            return View();
        }

        // POST: SYMGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,Name,Region,SubRegion,Status,LeaderId")] SYMGroup sYMGroup)
        {
            var ifLeader =  _context.Users.Where(u => u.Role == "Leader");
            if (ModelState.IsValid)
            {
                _context.Add(sYMGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");
            return View(sYMGroup);
        }

        // GET: SYMGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader");
            if (id == null) return NotFound();

            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (sYMGroup == null) return NotFound();

            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");
            return View(sYMGroup);
        }

        // POST: SYMGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Edit
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
                    var existing = await _context.SYMGroup.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Name = sYMGroup.Name;
                    existing.Region = sYMGroup.Region;
                    existing.SubRegion = sYMGroup.SubRegion;
                    existing.Status = sYMGroup.Status;
                    existing.LeaderId = sYMGroup.LeaderId;

                    // ✅ Removed _context.Update(sYMGroup) — not needed
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
            var groups = await _context.SYMGroup
                .Where(g => g.GroupId != currentGroupId && g.Status == "Active")
                .Select(g => new { g.GroupId, g.Name })
                .ToListAsync();

            return Json(groups); // ✅ was return View(groups)
        }
        // GET: SYMGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sYMGroup = await _context.SYMGroup
                .Include(s => s.Leader)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (sYMGroup == null)
            {
                return NotFound();
            }

            return View(sYMGroup);
        }

        // POST: SYMGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sYMGroup = await _context.SYMGroup.FindAsync(id);
            if (sYMGroup != null)
            {
                _context.SYMGroup.Remove(sYMGroup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SYMGroupExists(int id)
        {
            return _context.SYMGroup.Any(e => e.GroupId == id);
        }


        [HttpGet]
        public async Task<IActionResult> GetAvailableMembers()
        {
            var assignedUserIds = await _context.GroupMembers
                .Select(gm => gm.UserId)
                .ToListAsync();

            var available = await _context.Users
                .Where(u => u.Role == "Member"
                         && !assignedUserIds.Contains(u.Id)) 
                .Select(u => new { u.Id, u.FullName, u.Email, u.Status })
                .ToListAsync();

            return Json(available);
        }
        // POST: Add member to this group
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int userId)
        {
            bool alreadyExists = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

            if (alreadyExists)
            {
                TempData["Error"] = "Member already belongs to a group.";
                return RedirectToAction("Details", new { id = groupId });
            }

            var newMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                TotalEarnedPoints = 0
            };

            _context.GroupMembers.Add(newMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member added successfully!";
            return RedirectToAction("Details", new { id = groupId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferMember(int groupMemberId, int targetGroupId, int currentGroupId)
        {
            var member = await _context.GroupMembers.FindAsync(groupMemberId);

            if (member == null)
                return NotFound();

            // ✅ Check if already in target group
            bool alreadyInTarget = await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == member.UserId && gm.GroupId == targetGroupId);

            if (alreadyInTarget)
            {
                TempData["Error"] = "Member is already in that group.";
                return RedirectToAction("Details", new { id = currentGroupId });
            }

            member.GroupId = targetGroupId;
            _context.GroupMembers.Update(member);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member transferred successfully!";
            return RedirectToAction("Details", new { id = currentGroupId });
        }
    }
}
