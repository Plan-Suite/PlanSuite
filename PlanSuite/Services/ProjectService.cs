using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using System.Security.Claims;
using System.Text.Json;

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
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if(card != null)
            {
                card.ColumnId = model.ColumnId;
                m_Database.SaveChanges();
            }
        }

        public void EditCardDesc(EditCardDescModel model)
        {
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardDescription = model.Description;
                m_Database.SaveChanges();
            }
        }
    }
}
