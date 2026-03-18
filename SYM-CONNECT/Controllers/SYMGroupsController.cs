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
                .Include(s => s.GroupMembers)       // load members
                    .ThenInclude(m => m.User)       //load each member's User
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,Name,Region,SubRegion,Status,LeaderId")] SYMGroup sYMGroup)
        {
            var ifLeader = _context.Users.Where(u => u.Role == "Leader");

            if (id != sYMGroup.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.SYMGroup.FindAsync(id); //update existing data 
                    if (existing == null) return NotFound();

                    existing.Name = sYMGroup.Name;
                    existing.Region = sYMGroup.Region;
                    existing.SubRegion = sYMGroup.SubRegion;
                    existing.Status = sYMGroup.Status;
                    existing.LeaderId = sYMGroup.LeaderId;

                    _context.Update(sYMGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SYMGroupExists(sYMGroup.GroupId))
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
            ViewData["LeaderId"] = new SelectList(ifLeader, "Id", "FullName");
            return View(sYMGroup);
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
    }
}
