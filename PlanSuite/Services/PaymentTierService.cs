using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Services
{
    public class PaymentTierService
    {
        private readonly ProjectService m_ProjectService;

        public PaymentTierService(ProjectService projectService)
        {
            m_ProjectService = projectService;
        }

        public PaymentTier GetProjectTier(Project project)
        {
            return m_ProjectService.GetProjectTier(project);
        }

        public PaymentTier GetUserTier(ApplicationUser appUser)
        {
            return appUser.PaymentTier;
        }
    }
}
