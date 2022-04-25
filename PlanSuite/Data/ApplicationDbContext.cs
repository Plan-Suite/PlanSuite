using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Models.Persistent;
using System.Reflection;

namespace PlanSuite.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Project> Projects { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()).UseCollation("latin1_swedish_ci");
        }
    }
}