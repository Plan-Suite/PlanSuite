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
                string cardName = card.CardName;
                if (string.IsNullOrEmpty(card.CardName))
                {
                    cardName = "Empty Card Name";
                }
                string cardDesc = card.CardDescription;
                if (string.IsNullOrEmpty(card.CardDescription))
                {
                    cardDesc = "Click here to add a description.";
                }

                uint unixTime = 0;
                if(card.CardDueDate != null)
                {
                    unixTime = (uint)new DateTimeOffset((DateTime)card.CardDueDate).ToUnixTimeSeconds();
                    Console.WriteLine(unixTime);
                }

                GetCardReturnJson json = new GetCardReturnJson()
                {
                    Name = cardName,
                    MarkdownContent = Markdown.Parse(cardDesc).ReplaceLineEndings("<br/>"),
                    RawContent = cardDesc,
                    UnixTimestamp = unixTime,
                };
                return json;
            }
            return null;
        }

        public void EditCardDueDate(EditCardDueDateModel model)
        {
            // Convert Unix Timestamp to DateTime
            DateTime? dueDate = null;
            if(model.Timestamp > 0)
            {
                dueDate = DateTimeOffset.FromUnixTimeSeconds(model.Timestamp).UtcDateTime;
            }

            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardDueDate = dueDate;
                m_Database.SaveChanges();
            }
        }
    }
}
