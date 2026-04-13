using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SYM_CONNECT.Data;
using SYM_CONNECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SYM_CONNECT.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly AppDbContext _context;  //FOR  DB  PURPOSES

        public GroupMembersController(AppDbContext context)
        {
            _context = context;  //CONSTRUCTOR AREA
        }

        // GET: GroupMembers
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.GroupMembers.Include(g => g.Group).Include(g => g.User); //GET GROUPMEMBERS   INCLUDING THE USERS 
            return View(await appDbContext.ToListAsync()); //RETURN TO THE VIEW 
        }

        // GET: GroupMembers/Details/5
        public async Task<IActionResult> Details(int? id) //GET DETAILS AREA
        {
            if (id == null)
            {
                return NotFound(); //IF USERS ID DOESNT EXISTS
            }

            var groupMember = await _context.GroupMembers 
                .Include(g => g.Group)
                .ThenInclude(g => g.Leader)
                .Include(g => g.User)
                .ThenInclude(u => u.Attendances)
                 .ThenInclude(a => a.Event)
                .FirstOrDefaultAsync(m => m.GroupMemberId == id);
            if (groupMember == null)
            {
                return NotFound(); //IF NOT FOUND
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create

        public IActionResult Create() //GET SECTION CREATE FOR UI PURPOSES
        {
            var members = _context.Users  //GET USERS WHOS ROLE IS MEMBBER AND DISPLAY ITS USERNAME FULLNAME AND EMAIL
               .Where(u => u.Role == "Member")
               .Select(u => new { u.Id, u.FullName, u.Email })
               .ToList();

            //VIEW   DATA FOR UI PURPOSES
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name");
            ViewData["UserId"] = new SelectList(members, "Id", "Email");


            //VIEW   DATA FOR UI PURPOSES

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            ViewData["UsersJson"] = System.Text.Json.JsonSerializer.Serialize(members, options);

            return View();
        }

        // POST: GroupMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost] //FOR METHOD = POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupMemberId,GroupId,UserId")] GroupMember groupMember)
        {
            var hasGroup = await _context.GroupMembers //GET GROUPMEMBERS FROM THE USER MODEL TO ITS USERID
                .FirstOrDefaultAsync(m => m.UserId == groupMember.UserId);

            if (hasGroup != null)
            {
                TempData["Error"] = "This member is already assigned to a group."; //DOES THE MEMBER HAS EXISTING GROUP??
                return RedirectToAction(nameof(Index));  //THROW ERROR MESSAGE AND REDIRECT
            }

            if (ModelState.IsValid) //IF ITS VALID 
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var members = _context.Users.Where(u => u.Role == "Member");
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name", groupMember.GroupId);
            ViewData["UserId"] = new SelectList(members, "Id", "Email", groupMember.UserId);
            return View(groupMember); //FOR VIEW PURPOSES
        }
        // GET: GroupMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var members = _context.Users
       .Where(u => u.Role == "Member")
       .Select(u => new { u.Id, u.FullName, u.Email })
       .ToList(); //GET MEMBERS WHERE ROLE IS A MEMBER AND SHOW FULLNAME AND EMAIL


            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers.FindAsync(id); //FIND IN DB THE ID
            if (groupMember == null)
            {
                return NotFound(); //RETURN NOT FOUND IF NOT FOUND
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            ViewData["UsersJson"] = System.Text.Json.JsonSerializer.Serialize(members, options);
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
            var ifMember = _context.Users.Where(u => u.Role == "Member"); //IS IT  A MEMBER OR NOT? 

            if (id != groupMember.GroupMemberId)
            {
                return NotFound(); //THROW NOT FOUND IF NOT FOUND
            }
             //IF ALL CLEAR 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember); //SHOULD UPDATE THE DB
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) //TO CATCH  ERROR IF THE GROUPMEMBER DOESNT EXISTS ANYMORE
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
            //THIS IS A VIEWDATA FOR UI VIEW PURPOSES SECTION
            ViewData["GroupMemberId"] = new SelectList(ifMember, "Id", "FullName"); 
            ViewData["GroupId"] = new SelectList(_context.SYMGroup, "GroupId", "Name", groupMember.GroupId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", groupMember.UserId);
            return View(groupMember);
        }

        // GET: GroupMembers/Delete/5
        public async Task<IActionResult> Delete(int? id) //GET SELECTED ID TO DELETE
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers //FIND THE GROUP MEMBER IN THE DATABASE
                .Include(g => g.Group)
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.GroupMemberId == id);
            if (groupMember == null)
            {
                return NotFound(); //IF NOT  FOUND
            }

            return View(groupMember);
        }

        // POST: GroupMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) //DOES IT CLICK THE DELETE CONFIRMED? 
        {
            var groupMember = await _context.GroupMembers.FindAsync(id); //FIND THE SELECTED ID TO DELETE
            if (groupMember != null)
            {
                _context.GroupMembers.Remove(groupMember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id) //CHECK METHOD BOOLEAN IF TRUE THAT THE MEMBER HAS A GROUP ALREADY
        {
            return _context.GroupMembers.Any(e => e.GroupMemberId == id);
        }
    }
}
