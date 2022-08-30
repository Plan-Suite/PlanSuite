using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class EditOrganisationViewModel : BaseViewModel
    {
        public Organisation Organisation { get; set; }
        public EditOrganisationFormModel EditOrganisation = new EditOrganisationFormModel();
        public DeleteOrganisationModel DeleteOrganisation = new DeleteOrganisationModel();
    }

    public class EditOrganisationFormModel
    {
        public int OrganisationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class DeleteOrganisationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
    }
}
