using PlanSuite.Models.Persistent;
using Microsoft.AspNetCore.Mvc;

namespace PlanSuite.Models.Temporary
{
    public class JournalIndexViewModel : BaseViewModel
    {
        public List<JournalNote> PrivateNotes { get; set; }
    }

    public class JournalEntryWrite : BaseViewModel
    {
        public JournalNote? JournalNote { get; set; }
        public JournalEntryUpdateModel JournalEntryUpdate { get; set; }
    }

    public class JournalEntryUpdateModel
    {
        public int JournalId { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
