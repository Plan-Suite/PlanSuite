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
    }

    public class EditCardDueDateModel
    {
        public int CardId { get; set; }
        public uint Timestamp { get; set; }
    }
}
