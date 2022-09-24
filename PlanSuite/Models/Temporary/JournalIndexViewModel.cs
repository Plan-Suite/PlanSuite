using PlanSuite.Models.Persistent;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class JournalIndexViewModel : BaseViewModel
    {
        public List<JournalNote> PrivateNotes { get; set; }
    }

    public class JournalEntryWrite : BaseViewModel
    {
        public JournalNote? JournalNote { get; set; }
        public JournalEntryUpdateModel JournalEntryUpdate { get; set; } = new JournalEntryUpdateModel();
    }

    public class JournalEntryUpdateModel
    {
        public int JournalId { get; set; }
        public Guid OwnerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
