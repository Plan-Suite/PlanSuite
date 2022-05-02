using PlanSuite.Data;
using PlanSuite.Models.Temporary;

namespace PlanSuite.Services
{
    public class ProjectService
    {
        private readonly ApplicationDbContext m_Database;

        public ProjectService(ApplicationDbContext dbContext)
        {
            m_Database = dbContext;
        }

        public void MoveCard(MoveCardModel model)
        {
            throw new NotImplementedException();
        }
    }
}
