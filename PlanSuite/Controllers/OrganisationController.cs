using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using Stripe;

namespace PlanSuite.Controllers
{
    public class OrganisationController : Controller
    {
        private readonly OrganisationService m_OrganisationService;
        private readonly ILogger<OrganisationController> m_Logger;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ApplicationDbContext m_Database;
        private readonly SignInManager<ApplicationUser> m_SigninManager;
        private readonly AuditService m_AuditService;

        public OrganisationController(OrganisationService organisationService, ILogger<OrganisationController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext database, SignInManager<ApplicationUser> signInManager, AuditService auditService)
        {
            m_OrganisationService = organisationService;
            m_Logger = logger;
            m_UserManager = userManager;
            m_Database = database;
            m_SigninManager = signInManager;
            m_AuditService = auditService;
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, user, AuditLogType.Modified, organisation.Id);

            return View(model);
        }

        public async Task<IActionResult> SeeMembers(int orgId, int error = 0)
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


            SeeMembersModel model = new SeeMembersModel();
            var organisation = m_Database.Organizations.Where(org => org.Id == orgId).FirstOrDefault();
            if(organisation == null)
            {
                m_Logger.LogError($"Organisation {orgId} does not exist");
                return Redirect("/Organisation/SeeOrganisations");
            }

            model.Organisation = organisation;

            Guid userId = Guid.Parse(user.Id);
            
            var organisationMember = m_Database.OrganizationsMembership.Where(member => 
                member.OrganisationId == orgId &&
                member.UserId == userId &&
                member.Role >= ProjectRole.User).FirstOrDefault();
            if(organisationMember == null)
            {
                m_Logger.LogError($"User {user.Id} is not a member of orgId {orgId} or does not have the correct permissions");
                return Redirect("/Organisation/SeeOrganisations");
            }

            model.OrganisationMembership = organisationMember;

            m_Logger.LogInformation($"Grabbing org membership for orgId {orgId}");

            var organisationMemberships = m_Database.OrganizationsMembership.Where(member => member.OrganisationId == orgId).ToList();
            if (organisationMemberships != null && organisationMemberships.Count > 0)
            {
                foreach (var orgMembership in organisationMemberships)
                {
                    if (orgMembership.Role < ProjectRole.User)
                    {
                        continue;
                    }

                    var orgUser = await m_UserManager.FindByIdAsync(orgMembership.UserId.ToString());

                    switch(orgMembership.Role)
                    {
                        case ProjectRole.Owner:
                            model.Owners.Add(userId, orgUser.UserName);
                            break;
                        case ProjectRole.Admin:
                            model.Admins.Add(userId, orgUser.UserName);
                            break;
                        default:
                            model.Members.Add(userId, orgUser.UserName);
                            break;
                    }
                }
            }


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember([FromForm] AddOrganisationMemberModel addMember)
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

            Console.WriteLine($"AddMember: {addMember.OrganisationId} {addMember.Username} {addMember.Email}");

            var membership = new OrganisationMembership();
            membership.OrganisationId = addMember.OrganisationId;

            ApplicationUser newMember = null;
            if(!string.IsNullOrEmpty(addMember.Username))
            {
                newMember = await m_UserManager.FindByNameAsync(addMember.Username);
            }
            if (!string.IsNullOrEmpty(addMember.Email))
            {
                newMember = await m_UserManager.FindByEmailAsync(addMember.Email);
            }

            RouteValueDictionary routeValues = new RouteValueDictionary();
            if(newMember != null)
            {
                membership.UserId = Guid.Parse(newMember.Id);
                membership.Role = ProjectRole.User;
                await m_Database.OrganizationsMembership.AddAsync(membership);
                await m_Database.SaveChangesAsync();
                m_Logger.LogInformation($"Added {membership.UserId} to organisation {membership.OrganisationId}");
                routeValues.Add("success", 1);
                await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, user, AuditLogType.AddedMember, newMember.Id);
            }
            else
            {
                routeValues.Add("error", 1);
            }

            routeValues.Add("orgId", addMember.OrganisationId);
            return RedirectToAction(nameof(SeeMembers), routeValues);
        }

        public async Task<IActionResult> MakeAdmin(int orgId, Guid userId)
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

            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("orgId", orgId);
            var membership = m_Database.OrganizationsMembership.Where(member => member.UserId == userId && member.OrganisationId == orgId).FirstOrDefault();
            if(membership == null)
            {
                m_Logger.LogError($"User was null during MakeAdmin");
                routeValues.Add("error", 2);
                return RedirectToAction(nameof(SeeMembers), routeValues);
            }

            membership.Role = ProjectRole.Admin;
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, user, AuditLogType.MakeAdmin, membership.UserId.ToString());

            routeValues.Add("success", 2);
            return RedirectToAction(nameof(SeeMembers), routeValues);
        }

        public async Task<IActionResult> RemoveUser(int orgId, Guid userId)
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

            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("orgId", orgId);

            var membership = m_Database.OrganizationsMembership.Where(member => member.UserId == userId && member.OrganisationId == orgId).FirstOrDefault();
            if(membership == null)
            {
                // Not a member of organisation
                m_Logger.LogError($"User {user.Id} is not a member of orgId {orgId} when trying to be removed.");
                routeValues.Add("error", 2);
                return RedirectToAction(nameof(SeeMembers), routeValues);
            }

            if(membership.Role >= ProjectRole.Admin)
            {
                // User is currently an admin
                m_Logger.LogError($"User {user.Id} is an admin/owner of organisation {orgId} when trying to be removed.");
                routeValues.Add("error", 3);
                return RedirectToAction(nameof(SeeMembers), routeValues);
            }

            m_Database.OrganizationsMembership.Remove(membership);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, user, AuditLogType.RemovedMember, membership.UserId.ToString());

            routeValues.Add("success", 3);
            return RedirectToAction(nameof(SeeMembers), routeValues);
        }
    }
}
