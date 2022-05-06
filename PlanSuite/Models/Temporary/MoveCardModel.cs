using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class MoveCardModel
    {
        [Required]
        public int CardId { get; set; }

        [Required]
        public int ColumnId { get; set; }
    }
}
