using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;

namespace SYM_CONNECT.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly AppDbContext _context;

        public GroupMembersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GroupMembers
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.GroupMembers.Include(g => g.Group).Include(g => g.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: GroupMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers
                .Include(g => g.Group)
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.GroupMemberId == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create
        public IActionResult Create()
        {
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: GroupMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupMemberId,GroupId,UserId")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name", groupMember.GroupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", groupMember.UserId);
            return View(groupMember);
        }

        // GET: GroupMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers.FindAsync(id);
            if (groupMember == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name", groupMember.GroupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", groupMember.UserId);
            return View(groupMember);
        }

        // POST: GroupMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupMemberId,GroupId,UserId")] GroupMember groupMember)
        {
            if (id != groupMember.GroupMemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.GroupMemberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name", groupMember.GroupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", groupMember.UserId);
            return View(groupMember);
        }

        // GET: GroupMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers
                .Include(g => g.Group)
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.GroupMemberId == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // POST: GroupMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupMember = await _context.GroupMembers.FindAsync(id);
            if (groupMember != null)
            {
                _context.GroupMembers.Remove(groupMember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMembers.Any(e => e.GroupMemberId == id);
        }
    }
}
