using PlanSuite.Enums;
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
    }

    public class EditCardDueDateModel
    {
        public int CardId { get; set; }
        public uint Timestamp { get; set; }
        public int Priority { get; set; }
        public string AssigneeId { get; set; }
    }

    public class GetProjectMembers
    {
        public string CardOwner { get; set; }
        public List<string> CardAdmins { get; set; } = new List<string>();
        public List<string> CardMembers { get; set; } = new List<string>();
    }
}
