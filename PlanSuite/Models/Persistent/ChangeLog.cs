using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("change_log")]
    public class ChangeLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("version_name")]
        public string VersionName { get; set; }

        [Column("additions")]
        public string Additions { get; set; }

        [Column("changes")]
        public string Changes { get; set; }

        [Column("fixes")]
        public string Fixes { get; set; }
    }
}