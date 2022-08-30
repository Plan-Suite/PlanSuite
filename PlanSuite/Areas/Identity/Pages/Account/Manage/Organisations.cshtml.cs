using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Areas.Identity.Pages.Account.Manage
{
    public class OrganisationsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<OrganisationsModel> m_Logger;

        /// <summary>
        /// Organisations that the user is a member of.
        /// </summary>
        public Dictionary<OrganisationMembership, Organisation> Organisations { get; set; } = new Dictionary<OrganisationMembership, Organisation>();

        public OrganisationsModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext database, ILogger<OrganisationsModel> logger)
        {
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Database = database;
            m_Logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                string error = $"Unable to load user during OrganisationModel.OnGetAsync().";
                m_Logger.LogCritical(error);
                return NotFound(error);
            }

            Guid userId = Guid.Parse(m_UserManager.GetUserId(User));

            var organisationMemberships = m_Database.OrganizationsMembership.Where(member => member.UserId == userId).ToList();
            if(organisationMemberships.Count > 0)
            {
                foreach(var organisationMembership in organisationMemberships)
                {
                    if(organisationMembership.OrganisationId < 1)
                    {
                        m_Logger.LogError($"Organisation membership {organisationMembership.Id} returned an invalid organisation id of 0");
                        continue;
                    }

                    var organisation = m_Database.Organizations.Where(org => org.Id == organisationMembership.OrganisationId).FirstOrDefault();
                    if(organisation == null)
                    {
                        m_Logger.LogError($"Organisation membership {organisationMembership.Id} returned a non existant organisation of id {organisationMembership.OrganisationId}");
                        continue;
                    }

                    if(organisationMembership.Role < Enums.ProjectRole.User)
                    {
                        m_Logger.LogError($"Organisation membership {organisationMembership.Id} returned an invalid role");
                        continue;
                    }

                    if(!Organisations.ContainsKey(organisationMembership))
                    {
                        Organisations.Add(organisationMembership, organisation);
                    }
                }
            }

            return Page();
        }
    }
}
