using Microsoft.AspNetCore.Identity;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;

namespace PlanSuite.Services
{
    public class OrganisationService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ILogger<OrganisationService> m_Logger;
        private readonly AuditService m_AuditService;

        public OrganisationService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<OrganisationService> logger, AuditService auditService)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
            m_Logger = logger;
            m_AuditService = auditService;
        }

        public async Task<OrganisationErrorCode> OnCreateOrganisation(CreateOrganisationModel model)
        {
            // Get owner tier
            var owner = await m_UserManager.FindByIdAsync(model.OwnerId.ToString());
            if(owner == null)
            {
                m_Logger.LogError($"Owner was null during OnCreateOrganisation with id {model.OwnerId}");
                return OrganisationErrorCode.OwnerWasNull;
            }

            /*PaymentTier paymentTier = owner.PaymentTier;
            if(paymentTier == PaymentTier.Free)
            {
                m_Logger.LogError($"Cannot create organisation as {owner.FullName} is only free tier.");
                return OrganisationErrorCode.IsFreeTier;
            }

            // Check if owner owns more than 1 organisation, and check if they are only plus tier, if thats the case then we stop processing this method.
            int orgCount = m_Database.OrganizationsMembership.Where(member => member.UserId == model.OwnerId && member.Role == ProjectRole.Owner).Count();
            if (orgCount >= 1 && paymentTier <= PaymentTier.Plus)
            {
                m_Logger.LogError($"Cannot create organisation as {owner.FullName} is only plus tier and already owns an organisation.");
                return OrganisationErrorCode.IsPlusTier;
            }*/

            m_Logger.LogInformation($"Creating organisation {model.Name}");
            Organisation organisation = new Organisation
            {
                Name = model.Name,
                Description = model.Description,
                Tier = owner.PaymentTier
            };
            await m_Database.Organizations.AddAsync(organisation);
            await m_Database.SaveChangesAsync();

            m_Logger.LogInformation($"Creating organisation membership for {owner.FullName} with org id {organisation.Id}");
            OrganisationMembership membership = new OrganisationMembership
            {
                OrganisationId = organisation.Id,
                UserId = model.OwnerId,
                Role = ProjectRole.Owner
            };
            await m_Database.OrganizationsMembership.AddAsync(membership);
            m_Logger.LogInformation($"Saving org {organisation.Id}");
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, owner, AuditLogType.Created, organisation.Id);

            return OrganisationErrorCode.Success;
        }

        public async Task<OrganisationErrorCode> OnDeleteOrganisation(DeleteOrganisationModel model)
        {
            // Get owner tier
            var owner = await m_UserManager.FindByIdAsync(model.UserId.ToString());
            if (owner == null)
            {
                m_Logger.LogError($"User was null during OnDeleteOrganisation with id {model.UserId}");
                return OrganisationErrorCode.OwnerWasNull;
            }

            // Check if the organisation already exists
            var organisation = m_Database.Organizations.Where(org => org.Id == model.Id).FirstOrDefault();
            if(organisation == null)
            {
                m_Logger.LogError($"Organisation {model.Id} was null during OnDeleteOrganisation");
                return OrganisationErrorCode.OrgWasNull;
            }

            // Check if the userId is the owner
            var organisationMembership = m_Database.OrganizationsMembership.Where(member => 
                member.OrganisationId == model.Id
                && member.UserId == model.UserId
                && member.Role >= ProjectRole.Owner).FirstOrDefault();
            if (organisationMembership == null)
            {
                m_Logger.LogError($"User is not a member of or is not the owner of organisation {model.Id} during OnDeleteOrganisation");
                return OrganisationErrorCode.OrgWasNull;
            }

            m_Logger.LogInformation($"Deleting organisation {model.Name}");
            m_Database.Organizations.Remove(organisation);

            m_Logger.LogInformation($"Deleting organisation membership for {owner.FullName} with org id {model.Id}");
            m_Database.OrganizationsMembership.Remove(organisationMembership);

            m_Logger.LogInformation($"Reverting organisation projects for organisation {model.Id} back to original owner");
            var organisationProjects = m_Database.Projects.Where(project => project.OrganisationId == model.Id).ToList();
            if(organisationProjects.Count > 0)
            {
                foreach(var project in organisationProjects)
                {
                    project.OrganisationId = 0;
                }
            }

            m_Logger.LogInformation($"Saving organisation deletion to database");
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Organisation, owner, AuditLogType.Deleted, model.Id);

            return OrganisationErrorCode.Success;
        }
    }
}
