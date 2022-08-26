using PlanSuite.Models.Persistent;
using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class SeeMembersModel : BaseViewModel
    {
        public Organisation Organisation { get; set; }
        public OrganisationMembership OrganisationMembership { get; set; }
        public Dictionary<Guid, string> Owners = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Admins = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Members = new Dictionary<Guid, string>();
        public AddOrganisationMemberModel AddMember = new AddOrganisationMemberModel();
    }

    public class AddOrganisationMemberModel
    {
        public int OrganisationId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}
