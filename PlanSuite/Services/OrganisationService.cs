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

        public OrganisationService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<OrganisationService> logger)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
            m_Logger = logger;
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

            PaymentTier paymentTier = owner.PaymentTier;
            if(paymentTier == PaymentTier.Free)
            {
                m_Logger.LogError($"Cannot create organisation as {owner.UserName} is only free tier.");
                return OrganisationErrorCode.IsFreeTier;
            }

            // Check if owner owns more than 1 organisation, and check if they are only plus tier, if thats the case then we stop processing this method.
            int orgCount = m_Database.OrganizationsMembership.Where(member => member.UserId == model.OwnerId && member.Role == ProjectRole.Owner).Count();
            if (orgCount >= 1 && paymentTier <= PaymentTier.Plus)
            {
                m_Logger.LogError($"Cannot create organisation as {owner.UserName} is only plus tier and already owns an organisation.");
                return OrganisationErrorCode.IsPlusTier;
            }

            m_Logger.LogInformation($"Creating organisation");
            Organisation organisation = new Organisation
            {
                Name = model.Name,
                Description = model.Description,
                Tier = paymentTier
            };
            await m_Database.Organizations.AddAsync(organisation);

            m_Logger.LogInformation($"Creating organisation membership for {owner.UserName} with org id {organisation.Id}");
            OrganisationMembership membership = new OrganisationMembership
            {
                OrganisationId = organisation.Id,
                UserId = model.OwnerId,
                Role = ProjectRole.Owner
            };
            await m_Database.OrganizationsMembership.AddAsync(membership);

            m_Logger.LogInformation($"Saving org {organisation.Id}");
            await m_Database.SaveChangesAsync();
            return OrganisationErrorCode.Success;
        }
    }
}
