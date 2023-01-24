namespace PlanSuite.Models.Temporary
{
    public class TrelloBoard
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TrelloList> Lists { get; set; }
    }

    public class TrelloCard
    {
        public bool Closed { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public DateTime? Due { get; set; }
        public List<TrelloChecklist> Checklists { get; set; }

        public int CardId { get; set; }
    }

    public class TrelloList
    {
        public string Name { get; set; }
        public List<TrelloCard> Cards { get; set; }
    }

    public class TrelloChecklist
    {
        public string Name { get; set; }
        public List<TrelloChecklistItem> ChecklistItems { get; set; }
    }

    public class TrelloChecklistItem
    {
        public string Name { get; set; }
        public bool Complete { get; set; }
    }
}
