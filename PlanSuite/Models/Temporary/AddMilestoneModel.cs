using PlanSuite.Models.Persistent;
using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class AddMilestoneModel
    {
        public int ProjectId { get; set; }

        [Required]
        public string Title { get; set; }

        [MaxLength(5000)]
        public string Description { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class EditMilestoneModel
    {
        public int ProjectId { get; set; }
        public int MilestoneId { get; set; }

        [Required]
        public string Title { get; set; }

        [MaxLength(5000)]
        public string Description { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class GetMilestoneDataForEditingModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class ToggleMilestoneIsClosedModel
    {
        public int MilestoneId { get; set; }
    }

    public class GetToggleMilestoneIsClosedModel
    {
        public bool IsClosed { get; set; }
    }

    public class DeleteMilestoneModel
    {
        public int MilestoneId { get; set; }
        public int ProjectId { get; set; }
    }

    public class GetMilestonesModel
    {
        public List<ProjectMilestone> Milestones { get; set; }
    }

    public class MarkCompleteModel
    {
        public int ProjectId { get; set; }
    }
}
