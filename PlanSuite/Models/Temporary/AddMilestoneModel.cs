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

    public class GetCalendarTasksModel
    {
        public class CalendarTask
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public bool Completed { get; set; }
            public string BackgroundColor { get; set; } = "#3A5FDA"; // I dont like this one bit, would prefer if we could define this on the client-side
        }

        public List<CalendarTask> Events { get; set; }
    }

    public class CalendarTasksModel
    {
        public int Id { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string TimeZone { get; set; }
    }
}
