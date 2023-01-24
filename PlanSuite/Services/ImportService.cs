using PlanSuite.Interfaces;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;
using System.Text.Json;

namespace PlanSuite.Services
{
    public class ImportService : IImportService
    {
        private readonly ILogger<ImportService> m_Logger;

        public ImportService(ILogger<ImportService> logger)
        {
            m_Logger = logger;
        }

        public TrelloBoard ImportTrelloJson(ApplicationUser user, string json)
        {
            m_Logger.LogInformation($"Starting trello import for {user.FullName} ({user.Email})...");
            JsonDocument trelloJson = JsonDocument.Parse(json);

            TrelloBoard trelloBoard = new TrelloBoard();
            trelloBoard.Name = trelloJson.RootElement.GetProperty("name").ToString();
            trelloBoard.Description = trelloJson.RootElement.GetProperty("desc").ToString();

            trelloBoard.Lists = new List<TrelloList>();
            int listCount = trelloJson.RootElement.GetProperty("lists").GetArrayLength();
            m_Logger.LogInformation($"Parsing {listCount} trello lists");
            for (int listIndex = 0; listIndex < listCount; listIndex++)
            {
                var listItem = trelloJson.RootElement.GetProperty("lists")[listIndex];

                TrelloList list = new TrelloList();
                list.Name = listItem.GetProperty("name").ToString();
                list.Cards = new List<TrelloCard>();
                int cardCount = trelloJson.RootElement.GetProperty("cards").GetArrayLength();
                m_Logger.LogInformation($"Parsing {cardCount} trello cards");
                for (int i = 0; i < cardCount; i++)
                {
                    TrelloCard card = new TrelloCard();
                    var cardIndex = trelloJson.RootElement.GetProperty("cards")[i];

                    if(cardIndex.GetProperty("idList").ToString().ToLower() == listItem.GetProperty("id").ToString().ToLower())
                    {
                        card.Closed = cardIndex.GetProperty("closed").GetBoolean();
                        card.Description = cardIndex.GetProperty("desc").ToString();
                        card.Name = cardIndex.GetProperty("name").ToString();

                        if(cardIndex.GetProperty("due").ValueKind != JsonValueKind.Null)
                        {
                            DateTime time = DateTime.Parse(cardIndex.GetProperty("due").ToString());
                            card.Due = time;
                        }


                        card.Checklists = new List<TrelloChecklist>();
                        int checklistCount = cardIndex.GetProperty("idChecklists").GetArrayLength();
                        if (checklistCount > 0)
                        {
                            m_Logger.LogInformation($"Parsing {checklistCount} trello checklists for card {i}");
                            for (int x = 0; x < cardIndex.GetProperty("idChecklists").GetArrayLength(); x++)
                            {
                                int checklists = trelloJson.RootElement.GetProperty("checklists").GetArrayLength();
                                if (checklists > 0)
                                {
                                    for (int y = 0; y < checklists; y++)
                                    {
                                        var checklist = trelloJson.RootElement.GetProperty("checklists")[y];
                                        if (checklist.GetProperty("id").ToString().ToLower() == cardIndex.GetProperty("idChecklists")[x].ToString().ToLower())
                                        {
                                            TrelloChecklist trelloChecklist = new TrelloChecklist();
                                            trelloChecklist.Name = checklist.GetProperty("name").ToString();
                                            trelloChecklist.ChecklistItems = new List<TrelloChecklistItem>();

                                            int checkItemCount = checklist.GetProperty("checkItems").GetArrayLength();
                                            for (int z = 0; z < checkItemCount; z++)
                                            {
                                                var checkItem = checklist.GetProperty("checkItems")[z];
                                                var checklistItem = new TrelloChecklistItem();
                                                if (checkItem.GetProperty("state").ToString().Equals("incomplete"))
                                                {
                                                    checklistItem.Complete = false;
                                                }
                                                else
                                                {
                                                    checklistItem.Complete = true;
                                                }

                                                checklistItem.Name = checkItem.GetProperty("name").ToString();
                                                trelloChecklist.ChecklistItems.Add(checklistItem);
                                            }
                                            card.Checklists.Add(trelloChecklist);
                                        }
                                    }
                                }
                            }
                        }

                        list.Cards.Add(card);
                    }
                }
                trelloBoard.Lists.Add(list);
            }

            m_Logger.LogInformation($"Parsed trello board:\n{JsonUtility.ToJson(trelloBoard, true)}");

            return trelloBoard;
        }
    }
}
