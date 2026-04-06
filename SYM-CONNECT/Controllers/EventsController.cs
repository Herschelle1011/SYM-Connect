using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using SYM_CONNECT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
            var vm = new EventViewModel
            {
                Events = await  _context.Events
                .Where(u => u.IsCancelled == false)
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser).ToListAsync(),
                CancelledEvents = await _context.Events.Where(u => u.IsCancelled == true).ToListAsync(),
            };
            return View(vm);    

        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser)
                  .Include(e => e.AssignedGroups)       
        .ThenInclude(g => g.Leader)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            PopulateViewBag();

            var groups = await _context.SYMGroup
                   .Include(g => g.Leader)       
                   .Where(g => g.Status == "Active") 
                   .ToListAsync();

            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem { 
                
                Value  =   g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}"

            }).ToList();


            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model, List<int> AssignedGroupIds)
        {
            PopulateViewBag();
            await LoadDropdowns(); //get groups from db

            bool hasConflict = await IsEventScheduleConflict(model.EventDate, model.EndDate);
            if (hasConflict)
            {
                ModelState.AddModelError("EventDate",
                    "This schedule overlaps with an existing event.");
                TempData["Error"] = "Schedule Conflict";
                return View(model);
            }


            if (model.EventDate < DateTime.Now)
            {
                ModelState.AddModelError("EventDate",
                    "Event date cannot be in the past.");

                TempData["Error"] = "Date Input Error";
                return View(model);
            }

            //event date proper validation
            if (model.EndDate.HasValue && model.EndDate <= model.EventDate) //condition for end date
            {
                ModelState.AddModelError("EndDate", "End date must be after the event start date.");

                TempData["Error"] = "Date Input Error";
                return View(model);
            }



            bool exists = await _context.Events.AnyAsync(u => u.Title == model.Title);
            if (exists)
            {
                ModelState.AddModelError("Title", "Event title already exist, create another!");
                TempData["Error"] = "Title already exists";
                return View(model);
            }

            if (AssignedGroupIds != null && AssignedGroupIds.Any())
            {
                model.AssignedGroups = await _context.SYMGroup
                    .Where(g => AssignedGroupIds.Contains(g.GroupId))
                    .ToListAsync();
            }

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

                    model.IsCancelled = false;
                }
                else if (role == "Leader")
                {
                    model.CreatedBy = userId;
                    model.ApprovedBy = null;
                    model.ApprovalStatus = "Pending";
                }


                _context.Events.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event Added Successfully!"; //toast notification

                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            var groups = await _context.SYMGroup
                   .Include(g => g.Leader)
                   .Where(g => g.Status == "Active")
                   .ToListAsync();


            await LoadDropdowns();

            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
            .Include(e => e.AssignedGroups)
            .FirstOrDefaultAsync(e => e.EventId == id);

            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem
            {

                Value = g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}"

            }).ToList();


            if (eventItem == null)
                return NotFound();

            PopulateViewBag();


            return View(eventItem);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,EndDate,CreatedBy,ApprovalStatus,ApprovedBy, AssignedGroupId")] Event eventItem, List<int> AssignedGroupIds)
        {
            await LoadDropdowns();

            PopulateViewBag();

            bool hasConflict = await IsEventScheduleConflict(eventItem.EventDate, eventItem.EndDate, eventItem.EventId);

            if (hasConflict)
            {
                ModelState.AddModelError("EventDate",
                    "This schedule overlaps with an existing event.");
                TempData["Error"] = "Schedule Conflict";
                return View(eventItem);
            }

            if (eventItem.EventDate < DateTime.Now)
            {
                ModelState.AddModelError("EventDate",
                    "Event date cannot be in the past.");
                TempData["Error"] = "Date Input Error";
                return View(eventItem);
            }

            //event date proper validation
            if (eventItem.EndDate.HasValue && eventItem.EndDate <= eventItem.EventDate) //condition for end date
            {
                ModelState.AddModelError("EndDate", "End date must be after the event start date.");
                TempData["Error"] = "Date Input Error";
                return View(eventItem);
            }

 
            if (id != eventItem.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    //current   id event
                    var existing = await _context.Events
                        .Include(e => e.AssignedGroups)
                        .FirstOrDefaultAsync(e => e.EventId == id);

                    if (existing == null) return NotFound();

                    existing.Title = eventItem.Title;
                    existing.Description = eventItem.Description;
                    existing.EventDate = eventItem.EventDate;
                    existing.EndDate = eventItem.EndDate;
                    existing.ApprovalStatus = eventItem.ApprovalStatus;
                    existing.ApprovedBy = eventItem.ApprovedBy;

                    existing.AssignedGroups.Clear();
                    if (AssignedGroupIds != null && AssignedGroupIds.Any())
                    {
                        var selectedGroups = await _context.SYMGroup
                            .Where(g => AssignedGroupIds.Contains(g.GroupId))
                            .ToListAsync();

                        foreach (var group in selectedGroups)
                            existing.AssignedGroups.Add(group);
                    }
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event Updated Successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.EventId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.ApprovedBy);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email", eventItem.CreatedBy);

            await ReloadAssignedGroups(eventItem, AssignedGroupIds);


            return View(eventItem);
        }


        private async Task ReloadAssignedGroups(Event eventItem, List<int> AssignedGroupIds)
        {
            if (AssignedGroupIds != null && AssignedGroupIds.Any())
            {
                eventItem.AssignedGroups = await _context.SYMGroup
                    .Where(g => AssignedGroupIds.Contains(g.GroupId))
                    .ToListAsync();
            }
            else
            {
                eventItem.AssignedGroups = new List<SYMGroup>();
            }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) //deleted eventId
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
            }

            _context.Remove(eventItem);//removed/deleted
            await _context.SaveChangesAsync();
            TempData["Error"] = "Event permanently deleted!"; //toast notification

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }


        // GET: Events/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound();

            // Show a confirmation page before cancelling
            return View(eventItem);
        }

        // POST: Events/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound();

            eventItem.IsCancelled = true;
            eventItem.CancelledAt = DateTime.Now;

            _context.Update(eventItem);
            await _context.SaveChangesAsync();

            TempData["Warning"] = "Event Cancelled!"; //toast notification

            return RedirectToAction(nameof(Index));
        }


        //REACTIVATE SECTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var getid = await _context.Events.FindAsync(id); //get  id of  the selected  event

            if(getid == null)
            {
                return NotFound();
            }

            getid.IsCancelled = false; //CHANGE TO FALSE 

            getid.ApprovalStatus = "Pending";  //default  to pending 

            await _context.SaveChangesAsync();
            TempData["Success"] = "Reactivated Successfully!";
            return RedirectToAction(nameof(Index));

        }
        private void PopulateViewBag()
        {
            var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewBag.CurrentUser = currentUserEmail;
            ViewBag.CurrentUserId = currentUserId;

            ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["ApprovedBy"] = new SelectList(_context.Users, "Id", "Email");
        }

        private async Task LoadDropdowns()
        {
            var groups = await _context.SYMGroup
                .Include(g => g.Leader)
                .Where(g => g.Status == "Active")
                .ToListAsync();

            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem
            {
                Value = g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}"
            }).ToList();

     
        }

        public async Task<IActionResult> ExportPDF()
        {
            // Get ALL users from database
            var users = await _context.Events
                .OrderBy(u => u.Title)
                 .Include(e => e.CreatedByUser)
                  .Include(e => e.ApprovedByUser)
                   .Include(e => e.AssignedGroups)
                   .ThenInclude(e => e.Leader)
                .ToListAsync();

            // Return as PDF using the ExportPDF view
            return new ViewAsPdf("ExportPDF", users)
            {
                FileName = $"Users_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore
                                    .Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10
                }
            };
        }

        private async Task<bool> IsEventScheduleConflict(DateTime newStart, DateTime? newEnd, int? excludeEventId = null)
        {
            var events = await _context.Events
                .Where(e => e.IsCancelled == false &&
                            (excludeEventId == null || e.EventId != excludeEventId))
                .ToListAsync();

            foreach (var ev in events)
            {
                var existingStart = ev.EventDate;
                var existingEnd = ev.EndDate ?? ev.EventDate; 
                var checkEnd = newEnd ?? newStart;          

                bool overlaps = newStart < existingEnd && checkEnd > existingStart;

                System.Diagnostics.Debug.WriteLine(
           $"Checking: NEW [{newStart} → {checkEnd}] " +
           $"vs EXISTING [{existingStart} → {existingEnd}] " +
           $"| Overlap: {overlaps} | Event: {ev.Title}");


                if (overlaps) return true; // conflict found
            }

            return false; // no conflict
        }


    }
}