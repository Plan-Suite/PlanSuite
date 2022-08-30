namespace PlanSuite.Models.Temporary
{
    public class GetOrganisationMembers
    {
        public List<string> Owners { get; set; } = new List<string>();
        public List<string> Admins { get; set; } = new List<string>();
        public List<string> Members { get; set; } = new List<string>();
    }
}
