using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;
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

        public void EditCardName(EditCardNameModel model)
        {
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardName = model.Name;
                m_Database.SaveChanges();
            }
        }

        public GetCardReturnJson GetCardMarkdown(int cardId/*GetCardMarkdownModel model*/)
        {
            var card = m_Database.Cards.Where(card => card.Id == cardId).FirstOrDefault();
            if (card != null)
            {
                string cardDesc = card.CardDescription;
                if (string.IsNullOrEmpty(card.CardDescription))
                {
                    cardDesc = "Click here to add a description.";
                }
                GetCardReturnJson json = new GetCardReturnJson()
                {
                    MarkdownContent = Markdown.Parse(cardDesc).ReplaceLineEndings("<br/>"),
                    RawContent = cardDesc
                };
                return json;
            }
            return null;
        }
    }
}
