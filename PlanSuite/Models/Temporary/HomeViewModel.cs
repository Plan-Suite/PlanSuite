using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class HomeViewModel
    {
        public CreateProjectModel CreateProject { get; set; } = new CreateProjectModel();
        public List<Project> OwnedProjects { get; set; } = new List<Project>();
        public List<Project> MemberProjects { get; set; } = new List<Project>();
        public EditProjectModel EditProject { get; set; } = new EditProjectModel();
        public DeleteProjectModel DeleteProject { get; set; } = new DeleteProjectModel();

        public class CreateProjectModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
        }

        public class EditProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
        }

        public class DeleteProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
