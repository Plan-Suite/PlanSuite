using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

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
        public Dictionary<Guid, string> OrganisationMembers { get; set; } = new Dictionary<Guid, string>();
        public AddMemberModel AddMember { get; set; } = new AddMemberModel();
        public decimal UsedBudget { get; set; }
        public MarkCompleteModel MarkComplete = new MarkCompleteModel();
        public List<ChecklistItemModel> ChecklistItems = new List<ChecklistItemModel>();

        public class ChecklistItemModel
        {
            public int ChecklistItemCard { get; set; }
            public bool checklistItemTicked { get; set; }
        }

        public class AddMemberModel
        {
            public int ProjectId { get; set; }
            public Guid SenderId { get; set; }

            [Display(Name = "Email")]
            [DataType(DataType.EmailAddress)]
            public string? Email { get; set; }

            [Display(Name = "Organisation Member")]
            public Guid? UserId { get; set; }
        }

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

            [Required]
            [DisplayName("Name")]
            [DataType(DataType.Text)]
            public string Name { get; set; }

            [DisplayName("Content")]
            [DataType(DataType.MultilineText)]
            public string? Content { get; set; }

            [DisplayName("Assignee")]
            public Guid? Assignee { get; set; }

            [DisplayName("Due Date")]
            [DataType(DataType.Date)]
            public DateTime? DueDate { get; set; }

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
        public bool Completed {get;set;}
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
