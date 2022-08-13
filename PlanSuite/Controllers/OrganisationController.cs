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
        private readonly SignInManager<ApplicationUser> m_SigninManager;

        public OrganisationController(OrganisationService organisationService, ILogger<OrganisationController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext database, SignInManager<ApplicationUser> signInManager)
        {
            m_OrganisationService = organisationService;
            m_Logger = logger;
            m_UserManager = userManager;
            m_Database = database;
            m_SigninManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateOrganisationModel createOrganisation)
        {
            if(!m_SigninManager.IsSignedIn(User))
            {
                return Redirect($"/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Redirect($"/Home/Index?orgStatus={(int)OrganisationErrorCode.ModelStateInvalid}");
            }

            m_Logger.LogInformation($"CreateOrganisation: {createOrganisation.Name} {createOrganisation.Description} {createOrganisation.OwnerId}");
            OrganisationErrorCode errorCode = await m_OrganisationService.OnCreateOrganisation(createOrganisation);

            return Redirect($"/Home/Index?orgStatus={(int)errorCode}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] DeleteOrganisationModel deleteOrganisation)
        {
            if (!m_SigninManager.IsSignedIn(User))
            {
                return Redirect($"/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Redirect($"/Home/Index?orgStatus={(int)OrganisationErrorCode.ModelStateInvalid}");
            }

            m_Logger.LogInformation($"DeleteOrganisation: {deleteOrganisation.Id} {deleteOrganisation.Name}");
            OrganisationErrorCode errorCode = await m_OrganisationService.OnDeleteOrganisation(deleteOrganisation);

            return Redirect($"/Home/Index?orgStatus={(int)errorCode}");
        }

        public async Task<IActionResult> SeeOrganisations()
        {
            if (!m_SigninManager.IsSignedIn(User))
            {
                return Redirect($"/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if(user == null)
            {
                m_Logger.LogError($"User was null during GetUserAsync");
                return Redirect("/Home/Index");
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

        public async Task<IActionResult> EditOrganisation(int orgId)
        {
            if (!m_SigninManager.IsSignedIn(User))
            {
                return Redirect($"/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                m_Logger.LogError($"User was null during GetUserAsync");
                return Redirect("/Home/Index");
            }

            if(orgId < 1)
            {
                m_Logger.LogError($"OrganisationId was 0 (user: {user.Id})");
                return Redirect("/Home/Index");
            }

            var organisation = m_Database.Organizations.Where(org => org.Id == orgId).FirstOrDefault();
            if(organisation == null)
            {
                m_Logger.LogError($"Organisation was null (user: {user.Id}, OrganisationId: {orgId})");
                return Redirect("/Home/Index");
            }

            var organisationMembership = m_Database.OrganizationsMembership.Where(member => 
                member.OrganisationId == orgId
                && member.UserId == Guid.Parse(user.Id)).FirstOrDefault();
            if(organisationMembership == null)
            {
                m_Logger.LogError($"User {user.Id} was not a member of organisation {orgId}");
                return Redirect("/Home/Index");
            }

            if(organisationMembership.Role < ProjectRole.Owner)
            {
                m_Logger.LogError($"User {user.Id} did not have the correct permissions to edit organisation {orgId}");
                return Redirect("/Home/Index");
            }

            EditOrganisationViewModel model = new EditOrganisationViewModel();
            model.Organisation = organisation;

            return View(model);
        }
    }
}
