using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class HomeViewModel
    {
        public CreateProjectModel CreateProject { get; set; } = new CreateProjectModel();
        public List<Project> Projects { get; set; }
        public EditProjectModel EditProject { get; set; } = new EditProjectModel();

        public class CreateProjectModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime? DueDate { get; set; }
        }

        public class EditProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime? DueDate { get; set; }
        }
    }
}
