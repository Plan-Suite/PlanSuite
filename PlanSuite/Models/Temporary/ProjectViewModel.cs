using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class ProjectViewModel : BaseViewModel
    {
        public Project Project;
        public List<Column> Columns = new List<Column>();
        public AddColumnModel AddColumn = new AddColumnModel();
        public AddCardModel AddCard = new AddCardModel();
        public ViewCardModel ViewCard = new ViewCardModel();
        public List<Card> Cards = new List<Card>();
        public Guid UserId;
        public ProjectRole ProjectRole;
        /*public List<CardChecklist> Checklists = new List<CardChecklist>();
        public List<ChecklistItem> ChecklistItems = new List<ChecklistItem>();*/

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

    public class LeaveProjectModel
    {
        public int ProjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class AddMemberModel
    {
        public int ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }

    public class EditColumnNameModel
    {
        public int ColumnId { get; set; }
        public string ColumnText { get; set; }
    }
}
