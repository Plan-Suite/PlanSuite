using Microsoft.AspNetCore.Mvc;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class HomeViewModel : BaseViewModel
    {
        public CreateProjectModel CreateProject { get; set; } = new CreateProjectModel();
        public CreateOrganisationModel CreateOrganisation { get; set; } = new CreateOrganisationModel();
        public List<ProjectModel> OwnedProjects { get; set; } = new List<ProjectModel>();
        public List<ProjectModel> MemberProjects { get; set; } = new List<ProjectModel>();
        public Dictionary<int, Organisation> OrganisationMap { get; set; } = new Dictionary<int, Organisation>();
        public EditProjectModel EditProject { get; set; } = new EditProjectModel();
        public DeleteProjectModel DeleteProject { get; set; } = new DeleteProjectModel();
        public Organisation ViewingOrganisation { get; set; }
        public OrganisationMembership CurrentOrganisationMembership { get; set; }
        public List<ItemList> Organisations { get; set; } = new List<ItemList>();
        public List<Card> DueTasks { get; set; } = new List<Card>();

        public class CreateProjectModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
            public int OrganisationId { get; set; } = 0;
            public string? Client { get; set; }
            public decimal Budget { get; set; }
            public ProjectBudgetType BudgetType { get; set; }
            public string BudgetUnit { get; set; }
        }

        public class EditProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
            public int Organisation { get; set; }
            public string? Client { get; set; }
            public decimal Budget { get; set; }
            public ProjectBudgetType BudgetType { get; set; }
            public string BudgetUnit { get; set; }
        }

        public class DeleteProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? DueDate { get; set; }
            public string? Client { get; set; }
            public decimal Budget { get; set; }
            public int ProjectBudgetType { get; set; }
            public string? BudgetMonetaryUnit { get; set; }
            public decimal ProjectUsedBudget { get; set; }
            public int OrganisationId { get; set; }
            public string OrganisationName { get; set; }
            public bool ProjectComplete { get; set; }
        }
    }
}
