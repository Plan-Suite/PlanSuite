using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Models.Persistent;
using System.Reflection;

namespace PlanSuite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<ProjectAccess> ProjectsAccess { get; set; }
        public DbSet<CardChecklist> CardChecklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
        public DbSet<Organisation> Organizations { get; set; }
        public DbSet<OrganisationMembership> OrganizationsMembership { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<JournalNote> JournalNotes { get; set; }
        public DbSet<SalesContact> SalesContacts { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogSubscription> BlogSubscriptions { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()).UseCollation("latin1_swedish_ci");
            builder.Entity<BlogPost>().HasIndex(post => post.Slug).IsUnique();
        }
    }
}