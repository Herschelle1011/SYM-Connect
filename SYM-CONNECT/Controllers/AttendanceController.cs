using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Rotativa.AspNetCore;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SYM_CONNECT.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context; //for  database purposes

        public AttendanceController(AppDbContext context)
        {
            _context = context; 
        }

        // GET: Attendance
        public async Task<IActionResult> Index()
        { 
            var appDbContext = _context.Attendances.Include(a => a.Event).Include(a => a.User);  //get attendace and include  events and user
            return View(await appDbContext.ToListAsync()); //using await async to load attendance   
        }

        // GET: Attendance/Details/5
        public async Task<IActionResult> Details(int? id) //GET  DETAILS
        {
            await LoadDropdowns();  //AUTO LOAD DROPDOWNS  METHOD


            if (id == null)  //IF  USER ID NOT  FOUND
            {
                return NotFound();
            }

            var attendance = await _context.Attendances 
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AttendanceId == id);  //GET ATTENDANCE INCLUDES EVENTS AND  A USER

            if (attendance == null)  //IF ATTENDANCE  NOT FOUND
            {
                return NotFound();
            }

            var eventList = ViewBag.EventList as SelectList;
            ViewBag.SelectedEvent = eventList?
                .FirstOrDefault(e => e.Value == attendance.EventId.ToString())?.Text; //TO DISPLAY EVENT LIST FOR VIEW PURPOSES
            return View(attendance);
        }

        // GET: Attendance/Create
        public async Task<IActionResult> Create(int? eventId = null)
        {
     
           await LoadDropdowns(eventId);  //LOAD  DROP DOWNS

            var model = new Attendance
            {
                AttendanceDate = DateTime.Now  //ATTENDACE DATE DEFAULT  TO CURRENT DATE NOW
            };


            return View(model);
        }

        // POST: Attendance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AttendanceId,UserId,EventId,AttendanceDate,PointsEarned")] Attendance attendance, GroupMember members) 
        {
            await LoadDropdowns(attendance.EventId); //LOAD DROPDOWNS
  
            bool alreadyAttended = await _context.Attendances.AnyAsync(a => a.UserId == attendance.UserId && 
                                           a.EventId == attendance.EventId); //DOES THE USER ALREADY ATTENDANDED? BOOLEAN

            var selectedEvent = await _context.Events //THE SELECTED EVENT GETS EVENTID
                .FirstOrDefaultAsync(e => e.EventId == attendance.EventId);









            if (selectedEvent == null) //IF NOT EXIST RETURN ERROR
            {
                ModelState.AddModelError("EventId", "Event not found.");
                TempData["Error"] = "Event Error";
                return View(attendance);
            }

            if (attendance.PointsEarned == 0 || attendance.PointsEarned < 0) //POINTS VALIDATIO PURPOSES
            {
                ModelState.AddModelError("PointsEarned",
                      "Please enter a valid number");
                TempData["Error"] = "Invalid Number";
                await LoadDropdowns(); //LOAD DROP DOWN
                return View(attendance);
            }


            if (alreadyAttended) { 
                ModelState.AddModelError("AttendanceDate", //DOES THE USER ALREADY ATTENDED? 
              "User already attended to this event.");
                TempData["Error"] = "User already attended.";
                await LoadDropdowns();
                return View(attendance); //RETURN TO VIEW
            }

            if (attendance.AttendanceDate < selectedEvent.EventDate) //PROPER VALIDATION IF EVENTDATE IS GREATER  THAN THE ATTENDANCEDATE SELECTED
            {

                ModelState.AddModelError("AttendanceDate", 
                  "Attendance cannot be recorded before the event start date.");  //SHOW ERROR MESSAGE
                TempData["Error"] = "Date Input Error";
                return View(attendance);
            }

            else if (attendance.AttendanceDate > selectedEvent.EndDate) // ELSE IF THE EVENTDATE IS DONE
            {
                ModelState.AddModelError("AttendanceDate",
                  "Attendance cannot be recorded selected event has ended.");
                TempData["Error"] = "Date Input Error";
                return View(attendance);
            }

            if (ModelState.IsValid)
            {
                // Check if this user has a GroupMember record
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(g => g.UserId == attendance.UserId);

                // Block attendance CURRENT USER if user has no group
                if (groupMember == null)
                {
                    ModelState.AddModelError("UserId",
                        "This member is not assigned to any group yet."); //SHOW ERROR MESSAGE
                    TempData["Error"] = "Member has no group assigned.";
                    await LoadDropdowns();
                    return View(attendance);
                }

                // THEN IT IS Safe to proceed
                _context.Attendances.Add(attendance);
                groupMember.TotalEarnedPoints += attendance.PointsEarned;
                _context.GroupMembers.Update(groupMember);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{attendance.PointsEarned} points added!";  //TOAST NOTIFICATION
                return RedirectToAction(nameof(Index));
            
        }
            //FOR VIEW DATA FOR THE VIEW PURPOSES
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", attendance.EventId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", attendance.UserId);
            return View(attendance);
        }


        [HttpGet]
        public async Task<IActionResult> GetMembersByEvent(int eventId)
        {
            var selectedEvent = await _context.Events
                .Include(e => e.AssignedGroups)
                    .ThenInclude(g => g.GroupMembers)
                        .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (selectedEvent == null)
                return Json(new List<object>());

            var members = selectedEvent.AssignedGroups
                .SelectMany(g => g.GroupMembers)
                .Where(gm => gm.User != null
                          && gm.User.Role == "Member"
                          && gm.User.Status == "Active")
                .Select(gm => new
                {
                    id = gm.UserId,
                    display = gm.User!.FullName + " - " + gm.User.Email
                })
                .DistinctBy(m => m.id)
                .ToList();

            return Json(members);
        }

        // GET: Attendance/Edit/5
        public async Task<IActionResult> Edit(int? id, [Bind("AttendanceId,UserId,EventId,AttendanceDate,PointsEarned")] Edit edit, int? eventId = null)
        {
         
            await LoadDropdowns(eventId); //ALWAYS LOAD DROPDOWNS

            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance == null)
            {
                return NotFound();
            }


            return View(attendance);
        }

        // POST: Attendance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AttendanceId,UserId,EventId,AttendanceDate,PointsEarned")] Attendance attendance)
        {

            var model = new Attendance
            {
                AttendanceDate = DateTime.Now,
            };

            await LoadDropdowns(attendance.EventId);

            bool alreadyAttended = await _context.Attendances.AnyAsync(
           a => a.UserId == attendance.UserId &&
                a.EventId == attendance.EventId &&
                a.AttendanceId != attendance.AttendanceId);  //current ID when edited avoid conflict

            var selectedEvent = await _context.Events.FindAsync(attendance.EventId);


            if (alreadyAttended)  
            {
                ModelState.AddModelError("AttendanceDate",
              "User already attended to this event.");
                TempData["Error"] = "User already attended.";
                await LoadDropdowns();
                return View(attendance);
            }

            if (attendance.AttendanceDate < selectedEvent.EventDate)
            {

                ModelState.AddModelError("AttendanceDate",
                  "Attendance cannot be recorded before the event start date.");
                TempData["Error"] = "Date Input Error";
                return View(attendance);
            }

            else if (attendance.AttendanceDate > selectedEvent.EndDate)
            {
                ModelState.AddModelError("AttendanceDate",
                  "Attendance cannot be recorded selected event has ended.");
                TempData["Error"] = "Date Input Error";
                return View(attendance);
            }


            if (id != attendance.AttendanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.AttendanceId))
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
            //ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", attendance.EventId);
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", attendance.UserId);
            return View(attendance);
        }



        [HttpGet]
        public async Task<IActionResult> GetArchiveInfo(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.AttendanceId == id);

            if (attendance == null)
                return NotFound();

            return Json(new
            {
                userName = attendance.User?.FullName ?? "Unknown",
                eventTitle = attendance.Event?.Title ?? "Unknown",
                points = attendance.PointsEarned
            });
        }



        // GET: Attendance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AttendanceId == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance != null)
            {
                // Deduct points first before removing
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(g => g.UserId == attendance.UserId);

                if (groupMember != null)
                {
                    groupMember.TotalEarnedPoints -= attendance.PointsEarned;
                    _context.GroupMembers.Update(groupMember);
                }

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
                TempData["Error"] = "Attendance record deleted and points deducted!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.AttendanceId == id);
        }


        public async Task<IActionResult> ExportPDF()
        {
            // Get ALL users from database
            var attendances = await _context.Attendances
                .Include(a => a.User)
                      .Include(a => a.Event)

                .OrderBy(u => u.PointsEarned)
                .ToListAsync();

            // Return as PDF using the ExportPDF view
            return new ViewAsPdf("ExportPDF", attendances)
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



        public async Task LoadDropdowns(int? eventId = null)
        {
            // 1. GROUPS DROPDOWN
            var groups = await _context.SYMGroup
                .Include(g => g.Leader)
                .Where(g => g.Status == "Active")
                .ToListAsync();

            ViewBag.AssignedGroup = groups.Select(g => new SelectListItem
            {
                Value = g.GroupId.ToString(),
                Text = $"{g.Name} — {g.Leader?.FullName ?? "No Leader"}"
            }).ToList();

            var now = DateTime.Now;

            var events = await _context.Events
                .Where(e => e.CancelledAt == null          // NOT CANCELLED
                         && e.EventDate <= now             // ALREADY STARTED
                         && e.EndDate != null              // HAS AN END DATE
                         && e.EndDate >= now)              // NOT YET ENDED = ONGOING
                .Select(e => new
                {
                    e.EventId,
                    Display = e.Title + " | " +
                        e.EventDate.ToString("MMM dd, yyyy hh:mm tt") + " - " +
                        (e.EndDate.HasValue
                            ? e.EndDate.Value.ToString("MMM dd, yyyy hh:mm tt")
                            : "No End Date")
                })
                .ToListAsync();

            ViewData["EventList"] = new SelectList(events, "EventId", "Display");

            // 3. MEMBERS DROPDOWN — ONLY IF AN EVENT IS SELECTED
            if (eventId.HasValue)
            {
                var selectedEvent = await _context.Events
                    .Include(e => e.AssignedGroups)
                        .ThenInclude(g => g.GroupMembers)
                            .ThenInclude(gm => gm.User)      // CORRECT: ThenInclude USER not UserId
                    .FirstOrDefaultAsync(e => e.EventId == eventId.Value);

                if (selectedEvent != null)
                {
                    var assignedGroups = selectedEvent.AssignedGroups.ToList();

                    // MEMBERS FROM ASSIGNED GROUPS ONLY
                    var memberList = assignedGroups
                        .SelectMany(g => g.GroupMembers)
                        .Where(gm => gm.User != null
                                  && gm.User.Role == "Member"
                                  && gm.User.Status == "Active")
                        .Select(gm => new
                        {
                            Id = gm.UserId,
                            Display = gm.User!.FullName + " - " + gm.User.Email,  // USE - NOT =
                            GroupId = gm.GroupId,
                            Group = gm.Group?.Name
                        })
                        .DistinctBy(m => m.Id)
                        .ToList();

                    // GROUP LIST FROM ASSIGNED GROUPS ONLY
                    var groupList = assignedGroups
                        .Select(g => new { g.GroupId, g.Name })
                        .ToList();

                    ViewData["MemberId"] = new SelectList(memberList, "Id", "Display");
                    ViewData["GroupList"] = new SelectList(groupList, "GroupId", "Name");
                }
            }
            else
            {
                // NO EVENT SELECTED — EMPTY DROPDOWNS
                ViewData["MemberId"] = new SelectList(Enumerable.Empty<object>());
                ViewData["GroupList"] = new SelectList(Enumerable.Empty<object>());
            }
        }


    }
}
