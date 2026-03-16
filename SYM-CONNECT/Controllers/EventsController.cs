using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SYM_CONNECT.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;


        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = _context.Events
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser);

            return View(await events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email");

            return View();
        }


        public IActionResult CreateModal()
        {
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value;

            ViewBag.CurrentUserName = fullName;

            return PartialView("_CreateEventModal", new Event());
        }


        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (ModelState.IsValid)
            {
                // get current logged in user
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                string role = User.FindFirst(ClaimTypes.Role).Value;





                if (role == "admin")
                {
                    model.CreatedBy = userId; 
                    model.ApprovedBy = userId;
                    model.ApprovalStatus = "Approved";
                }
                else if (role == "Leader")
                {
                    model.CreatedBy = userId;
                    model.ApprovedBy = null;
                    model.ApprovalStatus = "Pending";
                }


                _context.Events.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }

            return View();
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound();

            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.ApprovedBy);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.CreatedBy);

            return View(eventItem);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,CreatedBy,ApprovalStatus,ApprovedBy")] Event eventItem)
        {
            if (id != eventItem.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.EventId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.ApprovedBy);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.CreatedBy);

            return View(eventItem);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }



        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}