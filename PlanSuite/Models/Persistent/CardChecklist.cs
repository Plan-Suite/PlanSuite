using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("card_checklists")]
    public class CardChecklist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("checklist_name")]
        public string ChecklistName { get; set; }

        [Column("checklist_card")]
        public int ChecklistCard { get; set; }
    }
}