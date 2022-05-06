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
}
