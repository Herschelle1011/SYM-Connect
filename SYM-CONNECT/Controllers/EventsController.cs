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
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SYM_CONNECT.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly AppDbContext _context; //FOR DATABASE PURPOSES


        public EventsController(AppDbContext context)
        {
            _context = context; //FOR PUBLIC ACCESS CONSTRUCTOR
        }

        // GET: Events
        [HttpGet]
        public async Task<IActionResult> Index(int? month, int? year, int? cancelDay, int? cancelMonth, int? cancelYear, int? day)
        {
            //get events include 
            var eventsQuery = _context.Events
                .Where(u => u.IsCancelled == false)
                .Include(e => e.CreatedByUser)
                .Include(e => e.ApprovedByUser)
                .AsQueryable();

            if (month.HasValue) eventsQuery = eventsQuery.Where(e => e.EventDate.Month == month.Value);
            if (year.HasValue) eventsQuery = eventsQuery.Where(e => e.EventDate.Year == year.Value);
            if (day.HasValue) eventsQuery = eventsQuery.Where(e => e.EventDate.Day == day.Value);


            // ── Cancelled events with their own filter ────────────────────────
            var cancelledQuery = _context.Events
                .Where(u => u.IsCancelled == true)
                .AsQueryable();

            //for filter month and year day purposes
            if (cancelYear.HasValue) cancelledQuery = cancelledQuery.Where(e => e.CancelledAt.HasValue && e.CancelledAt.Value.Year == cancelYear.Value);
            if (cancelMonth.HasValue) cancelledQuery = cancelledQuery.Where(e => e.CancelledAt.HasValue && e.CancelledAt.Value.Month == cancelMonth.Value);
            if (cancelDay.HasValue) cancelledQuery = cancelledQuery.Where(e => e.CancelledAt.HasValue && e.CancelledAt.Value.Day == cancelDay.Value);

            var vm = new EventViewModel
            {
                Events = await eventsQuery.ToListAsync(),
                CancelledEvents = await cancelledQuery.ToListAsync(),
                SelectedMonth = month,
                SelectedYear = year,
                SelectedDay = day,
                CancelDay = cancelDay, //get day
                CancelMonth = cancelMonth, //get month
                CancelYear = cancelYear //get year
            };

            return View(vm);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id) //GET  DETAILS BY  ID
        { 
            if (id == null) //IF NOT EXISTS
                return NotFound();

            var eventItem = await _context.Events //GET EVENTS WITH INCLUDE VIA FOREIGN KEY 
                .Include(e => e.CreatedByUser) //GET USE FROM USER MODEL
                .Include(e => e.ApprovedByUser) //USER MODEL ALSO
                  .Include(e => e.AssignedGroups) //ASSIGNED GROUPS FROM SYMGROUPS INCLUDED
                          .ThenInclude(g => g.Leader) //GROUPS THAT INCLUDES THE NAME OF THE LEADER
                  .Include(e => e.EventHandlerBy) //WHO HANDLES THE EVENT? GET LEADERS

                .FirstOrDefaultAsync(e => e.EventId == id); 
            if (eventItem == null)
                return NotFound();

            return View(eventItem); //RETURN
        }

        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            PopulateViewBag(); //FOR DISPLAY PURPOSES

            var groups = await _context.SYMGroup
                   .Include(g => g.Leader)       
                   .Where(g => g.Status == "Active") 
                   .ToListAsync(); //GET GROUPS WITH STATUS ACTIVE DONT SHOW IF NOT

 
            ViewBag.AssignedLeader = await _context.Users
                .Where(u => u.Role == "Leader" && u.Status == "Active")  //GET ASSIGNED LEADER WHOS STATUS IS ACTIVE
                .Select(g => new SelectListItem
                {
                   
                    Value = g.Id.ToString(),
                    Text = $"{g.Email} — {g.FullName ?? "No Event Handler"}" //IF NO  ASSIGN LEADER SHOW THIS
                })
                .ToListAsync(); 




            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem {  //VIEW ASSIGNED GROUP IN SELECT LIST  WITH ITS LEADER
                
                Value  =   g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}"

            }).ToList();




            return View(); //RETURN VIEW
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model, List<int> AssignedGroupIds) 
        {
            if (!ModelState.IsValid) //FOR DEBUGGING PURPOSES 
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine(error.Key); //SHOW ERROR IF IT WONT SHOW
                }
            }


            PopulateViewBag();
            await LoadDropdowns(); //get groups from db

            if(model.EndDate == null)
            {
                ModelState.AddModelError("EndDate", "End Date is Required!"); //END DATE IS REQUIRED NOW
                TempData["Error"] = "Select a End Date";
            }

            bool checkIfAssigned = await _context.Events.AnyAsync(e => //DOES THE LEADER ALREADY ASSIGNED TO  ANOTHER EVENT???
            e.EventHandlerId == model.EventHandlerId
            && e.EventDate == model.EventDate && e.EndDate == model.EndDate);

            if (checkIfAssigned)
            {
                ModelState.AddModelError("EventHandlerId", "This Leader is already assigned to an another event"); //SHOW VALIDATION ERROR 
                TempData["Error"] = "Selected Leader Unavailable";
            }

            if (model.EventHandlerId == null) //IF NOTHING
            {
                ModelState.AddModelError("EventHandlerId", "Event handler is required."); //IT REQUIRES
                ViewBag.AssignedLeader = await _context.Users
                      .Where(u => u.Role == "Leader" && u.Status == "Active") //MUST BE ACTIVE
                      .Select(g => new SelectListItem
                      {
                          Value = g.Id.ToString(),
                          Text = $"{g.Email} — {g.FullName ?? "No Event Handler"}" //SHOW AGAIN
                      })
                   .ToListAsync();
                return View(model);
            }

            ViewBag.AssignedLeader = await _context.Users
          .Where(u => u.Role == "Leader" && u.Status == "Active")  //INCLUDES ACTIVE  SHOW LEADERS
          .Select(g => new SelectListItem
          {
              Value = g.Id.ToString(),
              Text = $"{g.Email} — {g.FullName ?? "No Event Handler"}"
          })
          .ToListAsync();

            bool hasConflict = await IsEventScheduleConflict(model.EventDate, model.EndDate);
            if (hasConflict)
            {
                ModelState.AddModelError("EventDate",
                    "This schedule overlaps with an existing event."); //DOES THE SCHEDULE  EXISTS? SHOW VALIDATION IF TRUE
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
                ModelState.AddModelError("EndDate", "End date must be after the event start date."); //END  DATE MUST ALWAYS AFTER   THE EVENT DATE 

                TempData["Error"] = "Date Input Error"; //SHOW  TOAST  NOTIFICATION
                return View(model);
            }

            bool exists = await _context.Events.AnyAsync(u => u.Title == model.Title); //DOES THE  EVENT  TITLE ALREADY EXISTS? 
            if (exists)
            {
                ModelState.AddModelError("Title", "Event title already exist, create another!");
                TempData["Error"] = "Title already exists";
                return View(model);
            }

            if (AssignedGroupIds != null && AssignedGroupIds.Any()) //WHERE ASSIGNED GROUP IS NOT NULL
            {
                model.AssignedGroups = await _context.SYMGroup // SHOW ASSIGNGROUPS LIST
                    .Where(g => AssignedGroupIds.Contains(g.GroupId))
                    .ToListAsync();
            }

            if (ModelState.IsValid)
            {
                // get current logged in user
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); 
                string role = User.FindFirst(ClaimTypes.Role).Value;
                //IF ROLE ADMIN
                if (role == "admin")
                {
                    model.CreatedBy = userId; 
                    model.ApprovedBy = userId;
                    model.ApprovalStatus = "Approved";

                    model.IsCancelled = false;
                }
                //IF THE ROLE IS A LEADER
                else if (role == "Leader")
                {
                    model.CreatedBy = userId;
                    model.ApprovedBy = null;
                    model.ApprovalStatus = "Pending"; //DEFAULT TO PENDING
                }


                _context.Events.Add(model);
                await _context.SaveChangesAsync(); //SAVE TO DB 
                TempData["Success"] = "Event Added Successfully!"; //toast notification

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id) //EDIT SECTION
        {
            //LOAD DROPDOWNS
            await LoadDropdowns();


            //FOR VIEW PURPOSES VIEWBAG ASSIGNEDLEADER
            ViewBag.AssignedLeader = await _context.Users
          .Where(u => u.Role == "Leader" && u.Status == "Active")
          .Select(g => new SelectListItem
          {
              Value = g.Id.ToString(),
              Text = $"{g.Email} — {g.FullName ?? "No Event Handler"}" //GET EMAIL AND ITS FULLNAME OF THE  LEADER
          })
          .ToListAsync();



            var groups = await _context.SYMGroup //GET GROUPS AND LEADER WHERE STATUS IS ACTIVE
                   .Include(g => g.Leader)
                   .Where(g => g.Status == "Active")
                   .ToListAsync();



            //ALWAYS LOAD DROPDOWNS
            await LoadDropdowns();

            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
            .Include(e => e.AssignedGroups)
            .FirstOrDefaultAsync(e => e.EventId == id);

            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem
            {

                Value = g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}" //GET NAME OF FULL NAME

            }).ToList();


            if (eventItem == null) //IF NOT FOUND?
                return NotFound();

            PopulateViewBag(); 


            return View(eventItem); //RETURN TO VIEW
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem, List<int> AssignedGroupIds)
        {
            await LoadDropdowns();
            PopulateViewBag();

     
            if (eventItem.EventHandlerId == null)
            {
                ModelState.AddModelError("EventHandlerId", "Event handler is required.");
                return View(eventItem);
            }

            bool hasConflict = await IsEventScheduleConflict(eventItem.EventDate, eventItem.EndDate, eventItem.EventId);
            if (hasConflict)
            {
                ModelState.AddModelError("EventDate", "This schedule overlaps with an existing event.");
                TempData["Error"] = "Schedule Conflict";
                return View(eventItem);
            }

            var isOngoingOrDone = eventItem.EventDate < DateTime.Now;

            if (eventItem.EventDate < DateTime.Now && !isOngoingOrDone)
            {
                ModelState.AddModelError("EventDate", "Event date cannot be in the past.");
                return View(eventItem);
            }


            if (eventItem.EndDate.HasValue && eventItem.EndDate <= eventItem.EventDate)
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
                    var existing = await _context.Events
                        .Include(e => e.AssignedGroups)
                        .FirstOrDefaultAsync(e => e.EventId == id);

                    if (existing == null) return NotFound();

                    // ✅ Basic fields
                    existing.Title = eventItem.Title;
                    existing.Description = eventItem.Description;
                    existing.EventDate = eventItem.EventDate;
                    existing.EndDate = eventItem.EndDate;
                    existing.ApprovalStatus = eventItem.ApprovalStatus;
                    existing.ApprovedBy = eventItem.ApprovedBy;

                    existing.EventHandlerId = eventItem.EventHandlerId;

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
            TempData["Success"] = "Account Restored Successfully!";
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
            ViewBag.AssignedLeader = await _context.Users
             .Where(u => u.Role == "Leader" && u.Status == "Active")
             .Select(g => new SelectListItem
             {
                 Value = g.Id.ToString(),
                 Text = $"{g.Email} — {g.FullName ?? "No Event Handler"}"
             })
             .ToListAsync();

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

        //FOR PDF EXPORTED SECTION!!!
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
                //FOR UI PURPOSES OF THE PDF FORMAT
                PageMargins = new Rotativa.AspNetCore.Options.Margins
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10
                }
            };
        }

        //DOES IT CONFLICT TO THE OTHER SAVED SCHEDULES? CHECKING AREA!!!!
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


                if (overlaps) return true; // DOES IT FOUND ANY CONFLICT??? 
            }

            return false; // no conflict //IF ONYL NOT RETURN TO  FALSE
        }


    }
}