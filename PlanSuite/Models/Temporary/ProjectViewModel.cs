using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.ComponentModel;

namespace PlanSuite.Models.Temporary
{
    public class ProjectViewModel : BaseViewModel
    {
        public Project Project;
        public Organisation Organisation;
        public List<Column> Columns = new List<Column>();
        public AddColumnModel AddColumn = new AddColumnModel();
        public AddCardModel AddCard = new AddCardModel();
        public ViewCardModel ViewCard = new ViewCardModel();
        public List<Card> Cards = new List<Card>();
        public List<ProjectMilestone> Milestones = new List<ProjectMilestone>();
        public Guid UserId;
        public ProjectRole ProjectRole;
        public AddMilestoneModel AddMilestone = new AddMilestoneModel();
        public EditMilestoneModel EditMilestone = new EditMilestoneModel();
        public DeleteMilestoneModel DeleteMilestone = new DeleteMilestoneModel();
        public PaymentTier PaymentTier = PaymentTier.Free;
        public Dictionary<Guid, string> ProjectMembers { get; set; } = new Dictionary<Guid, string>();
        public AddTaskModel AddTask { get; set; } = new AddTaskModel();

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

        public class AddTaskModel
        {
            public int ColumnId { get; set; }

            [DisplayName("Name")]
            public string Name { get; set; }

            [DisplayName("Content")]
            public string Content { get; set; }

            [DisplayName("Assignee")]
            public Guid Assignee { get; set; }

            [DisplayName("Due Date")]
            public DateTime DueDate { get; set; }

            [DisplayName("Priority")]
            public Priority Priority { get; set; }

            [DisplayName("Milestone")]
            public int MilestoneId { get; set; }
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
