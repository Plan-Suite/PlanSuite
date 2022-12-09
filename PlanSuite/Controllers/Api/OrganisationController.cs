using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using Microsoft.EntityFrameworkCore;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class OrganisationController : ControllerBase
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ILogger<OrganisationController> m_Logger;

        public OrganisationController(ApplicationDbContext database, UserManager<ApplicationUser> userManager, ILogger<OrganisationController> logger)
        {
            m_Database = database;
            m_UserManager = userManager;
            m_Logger = logger;
        }

        #region GET
        // GET: OrganisationMembers
        [HttpGet("GetOrganisationMembers")]
        public async Task<ActionResult<GetOrganisationMembers>> OrganisationMembers(int orgId)
        {
            GetOrganisationMembers organisationMembers = new GetOrganisationMembers();
            // Check if organisation actually exists.
            var organisation = await m_Database.Organizations.Where(org => org.Id == orgId).FirstOrDefaultAsync();
            if (organisation == null)
            {
                m_Logger.LogError($"Organisation {orgId} does not exist");
                return organisationMembers;
            }

            m_Logger.LogInformation($"Getting organisation members for {orgId}");

            await AddUsersToList(orgId, organisationMembers, ProjectRole.Owner);
            await AddUsersToList(orgId, organisationMembers, ProjectRole.Admin);
            await AddUsersToList(orgId, organisationMembers, ProjectRole.User);

            return organisationMembers;
        }
        #endregion

        private async Task AddUsersToList(int orgId, GetOrganisationMembers organisationMembers, ProjectRole role)
        {
            var members = await m_Database.OrganizationsMembership.Where(owner => owner.OrganisationId == orgId && owner.Role == role).ToListAsync();
            if (members.Count > 0)
            {
                m_Logger.LogInformation($"Get {members.Count} members of role {role} for organisation {orgId}");
                foreach (var member in members)
                {
                    var user = await m_UserManager.FindByIdAsync(member.UserId.ToString());
                    if (user == null)
                    {
                        continue;
                    }
                    switch(role)
                    {
                        case ProjectRole.Admin:
                            organisationMembers.Admins.Add(user.FullName);
                            break;
                        case ProjectRole.Owner:
                            organisationMembers.Owners.Add(user.FullName);
                            break;
                        default:
                            organisationMembers.Members.Add(user.FullName);
                            break;
                    }
                }
            }
        }
    }
}
