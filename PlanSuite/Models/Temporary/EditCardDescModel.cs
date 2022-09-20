using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class EditCardDescModel
    {
        [Required]
        public int CardId { get; set; }

        [Required]
        public string Description { get; set; }
    }

    public class EditCardNameModel
    {
        [Required]
        public int CardId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class GetCardMarkdownModel
    {
        [Required]
        public int CardId { get; set; }
    }

    public class GetCardReturnJson
    {
        public string Name { get; set; }
        public string MarkdownContent { get; set; }
        public string RawContent { get; set; }
        public uint UnixTimestamp { get; set; }
        public string? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public Priority Priority { get; set; }
        public Dictionary<Guid, string> Members { get; set; } = new Dictionary<Guid, string>();
        public List<CardChecklist> CardChecklists { get; set; } = new List<CardChecklist>();
        public List<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
        public Dictionary<int, string> ProjectMilestones { get; set; } = new Dictionary<int, string>();
        public int MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public List<AuditLogJsonModel> AuditLogs { get; set; } = new List<AuditLogJsonModel>();
    }

    public class AuditLogJsonModel
    {
        /// <summary>
        /// Username who made the audit log
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Localised message of the audit log
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Timestamp of the audit log
        /// </summary>
        public string Created { get; set; }
    }

    public class EditCardModel
    {
        public int CardId { get; set; }
        public uint Timestamp { get; set; }
        public int Priority { get; set; }
        public string AssigneeId { get; set; }
        public int MilestoneId { get; set; }
    }

    public class GetProjectMembers
    {
        public string CardOwner { get; set; }
        public List<string> CardAdmins { get; set; } = new List<string>();
        public List<string> CardMembers { get; set; } = new List<string>();
    }
}
