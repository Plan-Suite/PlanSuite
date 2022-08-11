using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class SeeOrganisationsModel : BaseViewModel
    {
        public Dictionary<OrganisationMembership, Organisation> Organisations { get; set; } = new Dictionary<OrganisationMembership, Organisation>();
    }
}
