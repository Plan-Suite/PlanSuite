using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public partial class ProjectViewModel
    {
        public Project Project;
        public List<Column> Columns = new List<Column>();
        public AddColumnModel AddColumn = new AddColumnModel();
        public AddCardModel AddCard = new AddCardModel();
        public ViewCardModel ViewCard = new ViewCardModel();
        public List<Card> Cards = new List<Card>();

        public class AddColumnModel
        {
            public int ProjectId { get; set; }
            public string Name { get; set; }
        }

        public class ViewCardModel
        {
            public int CardId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
