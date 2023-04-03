using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("cards")]
    public class Card
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("column_id")]
        public int ColumnId { get; set; }

        [Column("card_name")]
        public string CardName { get; set; }

        [Column("card_description")]
        public string? CardDescription { get; set; }

        [Column("card_start_date")]
        public DateTime? CardStartDate { get; set; }

        [Column("card_due_date")]
        public DateTime? CardDueDate { get; set; }

        [Column("card_priority")]
        public Priority CardPriority { get; set; }

        [Column("card_assignee")]
        public Guid CardAssignee { get; set; }

        [Column("card_milestone")]
        public int CardMilestone { get; set; }

        [Column("card_created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("card_is_finished")]
        public bool IsFinished { get; set; }

        [Column("card_view")]
        public ProjectIndexView DefaultView { get; set; }

        [Column("card_budget")]
        public decimal Budget { get; set; }
    }

    [Table("task_dependencies")]
    public class TaskDependency
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("task_to_depend_on")]
        public int TaskToDependOn { get; set; }
    }
}