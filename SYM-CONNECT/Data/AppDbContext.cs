using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using SYM_CONNECT.Models;
namespace SYM_CONNECT.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        // Tables in the database
        public DbSet<Users> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<SYMGroup> SYMGroup { get; set; } 

        public DbSet<GroupMember> GroupMembers { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedByUser) //every event has one created by the user
                .WithMany() //can create many events by specific user
                .HasForeignKey(e => e.CreatedBy) //created by via foreignkey to usersID existed in the users table 
                .OnDelete(DeleteBehavior.NoAction); //if user is inactive events wont get deleted automatically prevents error 

            modelBuilder.Entity<Event>() 
                .HasOne(e => e.ApprovedByUser) // one event one approver
                .WithMany() //approver can approve many events 
                .HasForeignKey(e => e.ApprovedBy) //approved by via foreignkey to usersID existed in the users table 
                .OnDelete(DeleteBehavior.NoAction); //If administrator is deleted events approved will remain

            modelBuilder.Entity<SYMGroup>()
       .HasOne(g => g.Leader)
       .WithMany()
       .HasForeignKey(( g) => g.LeaderId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMembers)
                .HasForeignKey(( gm) => gm.GroupId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId);

            modelBuilder.Entity<Event>()
    .HasMany(e => e.AssignedGroups)
    .WithMany(g => g.Events)
    .UsingEntity(j => j.ToTable("EventGroups"));

        }
    }
}
