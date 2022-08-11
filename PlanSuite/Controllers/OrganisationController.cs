using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers
{
    public class OrganisationController : Controller
    {
        private readonly OrganisationService m_OrganisationService;
        private readonly ILogger<OrganisationController> m_Logger;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ApplicationDbContext m_Database;

        public OrganisationController(OrganisationService organisationService, ILogger<OrganisationController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext database)
        {
            m_OrganisationService = organisationService;
            m_Logger = logger;
            m_UserManager = userManager;
            m_Database = database;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateOrganisationModel createOrganisation)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction($"/Home/Index?orgStatus={(int)OrganisationErrorCode.ModelStateInvalid}");
            }

            m_Logger.LogInformation($"CreateOrganisation: {createOrganisation.Name} {createOrganisation.Description} {createOrganisation.OwnerId}");
            OrganisationErrorCode errorCode = await m_OrganisationService.OnCreateOrganisation(createOrganisation);

            return RedirectToAction($"/Home/Index?orgStatus={(int)errorCode}");
        }

        public async Task<IActionResult> SeeOrganisations()
        {
            var user = await m_UserManager.GetUserAsync(User);
            if(user == null)
            {
                m_Logger.LogError($"User was null during GetUserAsync");
                return RedirectToAction("/Home/Index");
            }

            SeeOrganisationsModel model = new SeeOrganisationsModel();
            Guid userId = Guid.Parse(user.Id);
            m_Logger.LogInformation($"Grabbing org memberships for user {userId}");

            var organisationMemberships = m_Database.OrganizationsMembership.Where(member => member.UserId == userId).ToList();
            if(organisationMemberships != null && organisationMemberships.Count > 0)
            {
                foreach(var orgMembership in organisationMemberships)
                {
                    if(orgMembership.OrganisationId < 1)
                    {
                        continue;
                    }

                    if (orgMembership.Role < ProjectRole.User)
                    {
                        continue;
                    }

                    var organisation = m_Database.Organizations.Where(org => org.Id == orgMembership.OrganisationId).FirstOrDefault();
                    if (organisation == null)
                    {
                        continue;
                    }

                    if(!model.Organisations.ContainsKey(orgMembership))
                    {
                        m_Logger.LogInformation($"Grabbing org membership for user {userId} for org {organisation.Id}");
                        model.Organisations.Add(orgMembership, organisation);
                    }
                }
            }

            
            return View(model);
        }
    }
}
