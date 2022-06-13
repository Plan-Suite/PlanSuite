using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("checklist_items")]
    public class ChecklistItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("checklist_id")]
        public int ChecklistId { get; set; }

        [Column("item_name")]
        public string ItemName { get; set; }

        [Column("item_ticked")]
        public bool ItemTicked { get; set; }

        [Column("item_index")]
        public int ItemIndex { get; set; }
    }
}