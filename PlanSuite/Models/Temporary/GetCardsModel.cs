using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
	public class GetCardsModel
    {
        public List<GetCardModelCardJson> Cards { get; set; } = new List<GetCardModelCardJson>();
    }

    public class GetCardModelCardJson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
//2022-11-21T16:52:39.2962869+00:00
//YYYY-MM-DDTHH:mm:ss.sssZ