namespace PlanSuite.Models.Temporary
{
    public class CreateOrganisationModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
    }
}
