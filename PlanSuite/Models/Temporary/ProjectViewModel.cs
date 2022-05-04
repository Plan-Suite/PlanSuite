using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class ProjectViewModel
    {
        public Project Project;
        public List<Column> Columns = new List<Column>();
        public AddColumnModel AddColumn = new AddColumnModel();
        public List<Card> Cards = new List<Card>();

        public class AddColumnModel
        {
            public int ProjectId { get; set; }
            public string Name { get; set; }
        }
    }
}
