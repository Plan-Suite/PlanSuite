using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;

namespace PlanSuite.Interfaces
{
    public interface IImportService
    {
        TrelloBoard ImportTrelloJson(ApplicationUser user, string json);
    }
}
