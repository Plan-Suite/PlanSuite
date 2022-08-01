using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;

namespace PlanSuite.Services
{
    public class ProjectService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;

        public ProjectService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
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

        public async Task<GetCardReturnJson> GetCardMarkdown(int cardId)
        {
            var card = m_Database.Cards.Where(card => card.Id == cardId).FirstOrDefault();
            if (card != null)
            {
                var column = m_Database.Columns.Where(c => c.Id == card.ColumnId).FirstOrDefault();
                if(column == null)
                {
                    return null;
                }

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

                string assignee = "NOBODY";
                string assigneeId = "0";
                ApplicationUser user = await m_UserManager.FindByIdAsync(card.CardAssignee.ToString());
                if (user != null)
                {
                    assignee = user.UserName;
                    assigneeId = user.Id;
                }

                uint unixTime = 0;
                if(card.CardDueDate != null)
                {
                    unixTime = (uint)new DateTimeOffset((DateTime)card.CardDueDate).ToUnixTimeSeconds();
                    Console.WriteLine(unixTime);
                }

                // get project members
                int projId = column.ProjectId;
                Dictionary<Guid, string> members = new Dictionary<Guid, string>();
                var project = m_Database.Projects.Where(p => p.Id == projId).FirstOrDefault();
                if (project == null)
                {
                    return null;
                }

                var owner = GetProjectOwner(project);
                if (owner != null)
                {
                    members.Add(Guid.Parse(owner.Id), owner.UserName);
                }

                var projMembers = GetProjectUsers(projId);
                foreach(var member in projMembers)
                {
                    if(!members.ContainsKey(member.Key))
                    {
                        members.Add(member.Key, member.Value);
                    }
                }

                // get card checklists
                List<ChecklistItem> items = new List<ChecklistItem>();

                var cardChecklists = m_Database.CardChecklists.Where(checklist => checklist.ChecklistCard == cardId).ToList();
                if(cardChecklists != null && cardChecklists.Count > 0)
                {
                    foreach(var checklist in cardChecklists)
                    {
                        var checklistItems = m_Database.ChecklistItems.Where(item => item.ChecklistId == checklist.Id).OrderBy(item => item.ItemIndex).ToList();
                        if(checklistItems != null && checklistItems.Count > 0)
                        {
                            items.AddRange(checklistItems);
                        }
                    }
                }
                if(cardChecklists == null)
                {
                    cardChecklists = new List<CardChecklist>();
                }

                // return
                GetCardReturnJson json = new GetCardReturnJson()
                {
                    Name = cardName,
                    MarkdownContent = Markdown.Parse(cardDesc),
                    RawContent = cardDesc,
                    UnixTimestamp = unixTime,
                    AssigneeName = assignee,
                    AssigneeId = assigneeId,
                    Priority = card.CardPriority,
                    Members = members,
                    CardChecklists = cardChecklists,
                    ChecklistItems = items
                };
                return json;
            }
            return null;
        }

        public bool DeleteChecklistItem(DeleteChecklistItemModel model)
        {
            Console.WriteLine($"Deleting checklistitem {model.ChecklistItemId}");
            // Grab checklist item from database
            var checklistItem = m_Database.ChecklistItems.Where(item => item.Id == model.ChecklistItemId).FirstOrDefault();
            if (checklistItem == null)
            {
                return false;
            }

            m_Database.ChecklistItems.Remove(checklistItem);
            m_Database.SaveChanges();
            return true;
        }

        public CardChecklist AddChecklist(AddChecklistModel model)
        {
            Console.WriteLine($"Adding checklist {model.Name} to card {model.Id}");
            // Add checklist to card
            CardChecklist checklist = new CardChecklist();
            checklist.ChecklistCard = model.Id;
            checklist.ChecklistName = model.Name;

            m_Database.CardChecklists.Add(checklist);
            m_Database.SaveChanges();
            return checklist;
        }

        internal ActionResult<GetMilestoneDataForEditingModel> GetMilestoneInfoForEditingAsync(int id)
        {
            Console.WriteLine($"Grabbing milestone data for id {id}");

            GetMilestoneDataForEditingModel model = new GetMilestoneDataForEditingModel();
            var milestone = m_Database.ProjectMilestones.Where(item => item.Id == id).FirstOrDefault();
            if(milestone == null)
            {
                return model;
            }

            Console.WriteLine($"Returning milestone data for id {id}");
            model.Title = milestone.Title;
            model.Description = milestone.Description;
            model.DueDate = milestone.DueDate;
            return model;
        }

        public ActionResult<GetMilestonesModel> GetMilestones(int id)
        {
            GetMilestonesModel model = new GetMilestonesModel();
            model.Milestones = new List<ProjectMilestone>();

            var milestones = m_Database.ProjectMilestones.Where(item => item.ProjectId == id).ToList();
            if (milestones == null || milestones.Count < 1)
            {
                return model;
            }

            model.Milestones = milestones;
            return model;
        }

        internal async Task DeleteMilestoneAsync(DeleteMilestoneModel model)
        {
            Console.WriteLine($"Deleting milestone {model.MilestoneId} 1");
            // Grab checklist item from database
            var milestone = m_Database.ProjectMilestones.Where(item => item.Id == model.MilestoneId).FirstOrDefault();
            if (milestone == null)
            {
                return;
            }

            Console.WriteLine($"Deleting milestone {model.MilestoneId} 2");
            m_Database.ProjectMilestones.Remove(milestone);
            await m_Database.SaveChangesAsync();
        }

        public async Task<ActionResult<GetToggleMilestoneIsClosedModel>> ToggleMilestoneIsClosedAsync(ToggleMilestoneIsClosedModel model)
        {
            Console.WriteLine($"Toggling milestone closed state for {model.MilestoneId}");

            GetToggleMilestoneIsClosedModel closedModel = new GetToggleMilestoneIsClosedModel();
            var milestone = m_Database.ProjectMilestones.Where(item => item.Id == model.MilestoneId).FirstOrDefault();
            if (milestone == null)
            {
                return closedModel;
            }

            milestone.IsClosed = !milestone.IsClosed;
            milestone.LastUpdated = DateTime.Now;
            
            // Save changes
            await m_Database.SaveChangesAsync();

            Console.WriteLine($"Milestone {model.MilestoneId} IsClosed = {closedModel.IsClosed}");
            closedModel.IsClosed = milestone.IsClosed;
            return closedModel;
        }

        public bool DeleteChecklist(DeleteChecklistModel model)
        {
            // Delete all checklist items for checklist we are deleting
            Console.WriteLine($"Deleting checklist items for {model.ChecklistId}");
            var checklistItem = m_Database.ChecklistItems.Where(item => item.ChecklistId == model.ChecklistId).ToList();
            if (checklistItem == null)
            {
                return false;
            }
            m_Database.ChecklistItems.RemoveRange(checklistItem);

            // Delete checklist
            Console.WriteLine($"Deleting checklist {model.ChecklistId}");
            var checklist = m_Database.CardChecklists.Where(item => item.Id == model.ChecklistId).FirstOrDefault();
            if (checklist == null)
            {
                return false;
            }
            m_Database.CardChecklists.Remove(checklist);

            // Save changes
            m_Database.SaveChanges();
            return true;
        }

        internal async Task EditMilestoneAsync(EditMilestoneModel model)
        {
            Console.WriteLine($"Editing milestone {model.MilestoneId}");
            var milestone = m_Database.ProjectMilestones.FirstOrDefault(p => p.Id == model.MilestoneId);
            if (milestone == null)
            {
                Console.WriteLine($"No milestone with id {model.MilestoneId} found");
                return;
            }

            Console.WriteLine($"Title: {milestone.Title} -> {model.Title}");
            milestone.Title = model.Title;

            Console.WriteLine($"Description: {milestone.Description} -> {model.Description}");
            milestone.Description = model.Description;

            Console.WriteLine($"DueDate: {milestone.DueDate} -> {model.DueDate}");
            milestone.DueDate = model.DueDate;

            Console.WriteLine($"LastUpdated: {milestone.LastUpdated} -> {DateTime.Now}");
            milestone.LastUpdated = DateTime.Now;

            Console.WriteLine($"Saving milestone {model.MilestoneId}");
            await m_Database.SaveChangesAsync();
        }

        internal async Task AddMilestoneAsync(AddMilestoneModel model)
        {
            Console.WriteLine($"Adding milestone \"{model.Title}\" to project {model.ProjectId}");
            var milestone = new ProjectMilestone();
            milestone.ProjectId = model.ProjectId;
            milestone.Title = model.Title;
            milestone.Description = model.Description;
            milestone.LastUpdated = DateTime.Now;
            milestone.DueDate = model.DueDate;

            await m_Database.ProjectMilestones.AddAsync(milestone);
            await m_Database.SaveChangesAsync();
        }

        public bool ConvertChecklistItemToCard(ConvertChecklistItemModel model)
        {
            Console.WriteLine($"Converting checklistitem {model.ChecklistItemId} to card");
            // Grab checklist item from database
            var checklistItem = m_Database.ChecklistItems.Where(item => item.Id == model.ChecklistItemId).FirstOrDefault();
            if(checklistItem == null)
            {
                return false;
            }

            // get column Id
            var checklist = m_Database.CardChecklists.Where(list => list.Id == checklistItem.ChecklistId).FirstOrDefault();
            if (checklist == null)
            {
                return false;
            }

            var card = m_Database.Cards.Where(c => c.Id == checklist.ChecklistCard).FirstOrDefault();
            if (card == null)
            {
                return false;
            }

            var column = m_Database.Columns.Where(col => col.Id == card.ColumnId).FirstOrDefault();
            if (column == null)
            {
                return false;
            }

            // Create card
            AddCardModel addCard = new AddCardModel();
            addCard.ProjectId = column.ProjectId;
            addCard.ColumnId = column.Id;
            addCard.Name = checklistItem.ItemName;
            AddCard(addCard);

            // Delete checklist item
            m_Database.ChecklistItems.Remove(checklistItem);
            m_Database.SaveChanges();

            return true;
        }

        public void EditChecklistItemTickedState(EditChecklistItemTickedStateModel model)
        {
            Console.WriteLine($"Editing checklistitem {model.ChecklistItemId} tick state to {model.TickedState}");
            var checklistItem = m_Database.ChecklistItems.Where(item => item.Id == model.ChecklistItemId).FirstOrDefault();
            if (checklistItem != null)
            {
                checklistItem.ItemTicked = model.TickedState;
                m_Database.SaveChanges();
            }
        }

        public ChecklistItem AddChecklistItem(AddChecklistItemModel model)
        {
            int index = 0;
            var lastChecklistItem = m_Database.ChecklistItems.Where(item => item.ChecklistId == model.ChecklistId).OrderBy(item => item.ItemIndex).LastOrDefault();
            if(lastChecklistItem != null)
            {
                index = lastChecklistItem.ItemIndex + 1;
            }

            var checklistItem = new ChecklistItem
            {
                ChecklistId = model.ChecklistId,
                ItemIndex = index,
                ItemName = model.ItemText,
            };

            m_Database.ChecklistItems.Add(checklistItem);
            m_Database.SaveChanges();
            return checklistItem;
        }

        public void EditColumnTitle(EditColumnNameModel model)
        {
            var column = m_Database.Columns.Where(column => column.Id == model.ColumnId).FirstOrDefault();
            if (column != null)
            {
                column.Title = model.ColumnText;
                m_Database.SaveChanges();
            }
        }

        private ApplicationUser? GetProjectOwner(Project project)
        {
            var owner = m_Database.Users.Where(u => u.Id == project.OwnerId.ToString()).FirstOrDefault();
            if (owner == null)
            {
                return null;
            }
            return owner;
        }

        private Dictionary<Guid, string> GetProjectUsers(int projId)
        {
            var members = new Dictionary<Guid, string>();
            var projAccess = m_Database.ProjectsAccess.Where(a => a.ProjectId == projId).ToList();
            if (projAccess != null && projAccess.Count > 0)
            {
                foreach (var access in projAccess)
                {
                    var projMember = m_Database.Users.Where(u => u.Id == access.UserId.ToString()).FirstOrDefault();
                    if (projMember != null)
                    {
                        members.Add(Guid.Parse(projMember.Id), projMember.UserName);
                    }
                }
            }
            return members;
        }

        public AddMemberResponse AddMember(AddMemberModel model)
        {
            var project = m_Database.Projects.Where(project => project.Id == model.ProjectId).FirstOrDefault();
            if (project == null)
            {
                return AddMemberResponse.ServerError;
            }

            var role = GetUserProjectAccess(model.UserId, project);
            if (role < ProjectRole.Admin)
            {
                return AddMemberResponse.IncorrectRoles;
            }

            int maxMembers = 20;

            // get owner
            var owner = m_Database.Users.Where(user => user.Id == project.OwnerId.ToString()).FirstOrDefault();
            if (owner != null)
            {
                var tier = owner.PaymentTier;
                switch(tier)
                {
                    case PaymentTier.Plus:
                        maxMembers = 100;
                        break;
                    case PaymentTier.Pro:
                        // nobody's ever gonna reach this many project members so this is fine
                        maxMembers = 99999;
                        break;
                }
            }

            // get owners tier
            int count = m_Database.ProjectsAccess.Where(p => p.ProjectId == project.Id).ToList().Count;
            count = count + 1;
            if (count > maxMembers)
            {
                if(role == ProjectRole.Owner)
                {
                    return AddMemberResponse.IncorrectTierYou;
                }
                return AddMemberResponse.IncorrectTier;
            }

            var user = m_Database.Users.Where(user => user.UserName.Equals(model.Name)).FirstOrDefault();
            if (user == null)
            {
                return AddMemberResponse.NoUser;
            }

            var guid = Guid.Parse(user.Id);
            var access = m_Database.ProjectsAccess.Where(access => access.UserId == guid).FirstOrDefault();
            if (access != null)
            {
                return AddMemberResponse.AlreadyHasAccess;
            }

            access = new ProjectAccess();
            access.ProjectId = model.ProjectId;
            access.ProjectRole = ProjectRole.User;
            access.UserId = guid;

            Console.WriteLine($"SECURITY: User {model.UserId} added {user.UserName} to project {model.ProjectId}");
            m_Database.ProjectsAccess.Add(access);
            m_Database.SaveChanges();
            return AddMemberResponse.Success;
        }

        public void EditCardDueDate(EditCardDueDateModel model)
        {
            // Convert Unix Timestamp to DateTime
            DateTime? dueDate = null;
            if (model.Timestamp > 0)
            {
                dueDate = DateTimeOffset.FromUnixTimeSeconds(model.Timestamp).UtcDateTime;
            }

            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardDueDate = dueDate;
                card.CardPriority = (Priority)model.Priority;
                card.CardAssignee = Guid.Parse(model.AssigneeId);
                m_Database.SaveChanges();
            }
        }
        

        public void LeaveProject(LeaveProjectModel model)
        {
            var project = m_Database.Projects.Where(project => project.Id == model.ProjectId).FirstOrDefault();
            if (project == null)
            {
                return;
            }

            if(project.OwnerId == model.UserId)
            {
                return;
            }

            var access = m_Database.ProjectsAccess.Where(access => access.UserId == model.UserId).FirstOrDefault();
            if(access == null)
            {
                return;
            }

            Console.WriteLine($"SECURITY: User {model.UserId} left project {model.ProjectId}");
            m_Database.ProjectsAccess.Remove(access);
            m_Database.SaveChanges();
        }

        public GetProjectMembers GetProjectMembers(int cardId)
        {
            GetProjectMembers cardMembers = new GetProjectMembers();
            var project = m_Database.Projects.Where(card => card.Id == cardId).FirstOrDefault();
            if(project == null)
            {
                return cardMembers;
            }

            // Get card owner
            var owner = m_Database.Users.Where(user => user.Id == project.OwnerId.ToString()).FirstOrDefault();
            if (owner != null)
            {
                cardMembers.CardOwner = owner.UserName;
            }


            // Get card admins
            var admins = m_Database.ProjectsAccess.Where(access => access.ProjectId == project.Id && access.ProjectRole == ProjectRole.Admin).ToList();
            if (admins != null)
            {
                foreach(var admin in admins)
                {
                    var adminUser = m_Database.Users.Where(user => user.Id == admin.UserId.ToString()).FirstOrDefault();
                    if(adminUser != null)
                    {
                        cardMembers.CardAdmins.Add(adminUser.UserName);
                    }
                }

            }

            // Get card users
            var users = m_Database.ProjectsAccess.Where(access => access.ProjectId == project.Id && access.ProjectRole == ProjectRole.User).ToList();
            if (users != null)
            {
                foreach (var regUser in users)
                {
                    var regularUser = m_Database.Users.Where(user => user.Id == regUser.UserId.ToString()).FirstOrDefault();
                    if (regularUser != null)
                    {
                        cardMembers.CardMembers.Add(regularUser.UserName);
                    }
                }

            }
            return cardMembers;
        }

        public ProjectRole GetUserProjectAccess(ApplicationUser applicationUser, Project project)
        {
            // If user or project is null, return false.
            if(applicationUser == null || project == null)
            {
                return ProjectRole.None;
            }

            var guid = Guid.Parse(applicationUser.Id);
            return GetUserProjectAccess(guid, project);
        }

        public ProjectRole GetUserProjectAccess(Guid userId, Project project)
        {
            // If user or project is null, return false.
            if (project == null)
            {
                return ProjectRole.None;
            }

            // If the user is also the project owner, return true always
            if (project.OwnerId == userId)
            {
                return ProjectRole.Owner;
            }

            // Get the users project access
            var projectAccess = m_Database.ProjectsAccess.Where(access =>
                access.UserId == userId &&
                access.ProjectId == project.Id).FirstOrDefault();
            if (projectAccess == null)
            {
                return ProjectRole.None;
            }

            return projectAccess.ProjectRole;
        }

        public void AddCard(AddCardModel model)
        {
            Console.WriteLine($"Adding card {model.Name} to project {model.ProjectId}");
            var card = new Card();
            card.ColumnId = model.ColumnId;
            card.CardName = model.Name;
            m_Database.Cards.Add(card);

            m_Database.SaveChanges();
        }
    }
}
