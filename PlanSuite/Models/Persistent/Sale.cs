using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("sales")]
    public class Sale
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("owner_id")]
        public Guid OwnerId { get; set; }

        [Column("payment_tier")]
        public PaymentTier PaymentTier { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [Column("sale_state")]
        public SaleState SaleState { get; set; }

        [Column("sale_is_free")]
        public bool SaleIsFree { get; set; }
    }
}