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

    public class AddChecklistItemModel
    {
        public int ChecklistId { get; set; }
        public string ItemText { get; set; }
    }

    public class EditChecklistItemTickedStateModel
    {
        public int ChecklistItemId { get; set; }
        public bool TickedState { get; set; }
    }

    public class ConvertChecklistItemModel
    {
        public int ChecklistItemId { get; set; }
        public int ProjectId { get; set; }
    }

    public class DeleteChecklistItemModel
    {
        public int ChecklistItemId { get; set; }
    }

    public class DeleteChecklistModel
    {
        public int ChecklistId { get; set; }
    }

    public class AddChecklistModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
