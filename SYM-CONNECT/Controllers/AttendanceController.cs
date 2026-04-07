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
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Attendance
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Attendances.Include(a => a.Event).Include(a => a.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Attendance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            await LoadDropdowns();


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
            var eventList = ViewBag.EventList as SelectList;
            ViewBag.SelectedEvent = eventList?
                .FirstOrDefault(e => e.Value == attendance.EventId.ToString())?.Text;
            return View(attendance);
        }

        // GET: Attendance/Create
        public async Task<IActionResult> Create()
        {
     
           await LoadDropdowns();

            var model = new Attendance
            {
                AttendanceDate = DateTime.Now
            };


            return View(model);
        }

        // POST: Attendance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AttendanceId,UserId,EventId,AttendanceDate,PointsEarned")] Attendance attendance)
        {
            await LoadDropdowns();
  
            bool alreadyAttended = await _context.Attendances.AnyAsync(a => a.UserId == attendance.UserId && 
                                           a.EventId == attendance.EventId);

            var selectedEvent = await _context.Events
       .FirstOrDefaultAsync(e => e.EventId == attendance.EventId);


            if (selectedEvent == null)
            {
                ModelState.AddModelError("EventId", "Event not found.");
                TempData["Error"] = "Event Error";
                return View(attendance);
            }

            if (attendance.PointsEarned == 0 || attendance.PointsEarned < 0)
            {
                ModelState.AddModelError("PointsEarned",
                      "Please enter a valid number");
                TempData["Error"] = "Invalid Number";
                await LoadDropdowns();
                return View(attendance);
            }


            if (alreadyAttended) {
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

            if (ModelState.IsValid)
            {
                // Check if this user has a GroupMember record
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(g => g.UserId == attendance.UserId);

                // Block attendance if user has no group
                if (groupMember == null)
                {
                    ModelState.AddModelError("UserId",
                        "This member is not assigned to any group yet.");
                    TempData["Error"] = "Member has no group assigned.";
                    await LoadDropdowns();
                    return View(attendance);
                }

                // Safe to proceed
                _context.Attendances.Add(attendance);
                groupMember.TotalEarnedPoints += attendance.PointsEarned;
                _context.GroupMembers.Update(groupMember);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{attendance.PointsEarned} points added!";
                return RedirectToAction(nameof(Index));
            
        }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Description", attendance.EventId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", attendance.UserId);
            return View(attendance);
        }

        // GET: Attendance/Edit/5
        public async Task<IActionResult> Edit(int? id, [Bind("AttendanceId,UserId,EventId,AttendanceDate,PointsEarned")] Edit edit)
        {
         

            await LoadDropdowns();

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

            await LoadDropdowns();

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



        public async Task LoadDropdowns()
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
            

            var events = await _context.Events
              .Where(e => e.IsCancelled == false)
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

       

            var ifMember = _context.Users.Where(u => u.Role == "Member" && u.Status == "Active");

            var MemberList = ifMember.Select(m => new { m.Id, Display = m.FullName + " - " + m.Email }).ToList();
            var GetEvent = ifMember.Select(m => new { m.Id, Display = m.FullName + " - " + m.Email }).ToList();
            ViewData["EventList"] = new SelectList(events, "EventId", "Display");
            ViewData["MemberId"] = new SelectList(MemberList, "Id", "Display");



        }


     



    }
}
