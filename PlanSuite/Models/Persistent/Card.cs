﻿using System.ComponentModel.DataAnnotations;
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

        [Column("card_due_date")]
        public DateTime? CardDueDate { get; set; }
    }
}