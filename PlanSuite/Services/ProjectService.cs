using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly AuditService m_AuditService;

        public ProjectService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, AuditService auditService)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
            m_AuditService = auditService;
        }

        public async Task MoveCard(MoveCardModel model, ClaimsPrincipal user)
        {
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if(card != null)
            {
                card.ColumnId = model.ColumnId;
                await m_Database.SaveChangesAsync();
                await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.Moved, card.Id);
            }
        }

        public async Task EditCardDesc(EditCardDescModel model, ClaimsPrincipal user)
        {
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardDescription = model.Description;
                await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.ModifiedDescription, card.Id);
                await m_Database.SaveChangesAsync();
            }
        }

        public async Task EditCardName(EditCardNameModel model, ClaimsPrincipal user)
        {
            var card = m_Database.Cards.Where(card => card.Id == model.CardId).FirstOrDefault();
            if (card != null)
            {
                card.CardName = model.Name;
                await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.ModifiedDescription, card.Id);
                await m_Database.SaveChangesAsync();
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

                // get project milestones
                Dictionary<int, string> milestones = new Dictionary<int, string>();

                var projMilestones = m_Database.ProjectMilestones.Where(m => m.ProjectId == projId).ToList();
                foreach(var milestone in projMilestones)
                {
                    milestones.Add(milestone.Id, milestone.Title);
                }

                // get current milestone
                int currentMilestoneId = 0;
                string currentMilestoneName = string.Empty;
                if(card.CardMilestone > 0)
                {
                    Console.WriteLine($"card {card.Id} milestone is {card.CardMilestone}");
                    var ms = milestones.Where(m => m.Key == card.CardMilestone)?.FirstOrDefault();
                    if(ms != null)
                    {
                        Console.WriteLine($"currentMilestoneId = {ms.Value.Key}");
                        currentMilestoneId = ms.Value.Key;
                        Console.WriteLine($"currentMilestoneName = {ms.Value.Value}");
                        currentMilestoneName = ms.Value.Value;
                    }
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
                    ChecklistItems = items,
                    ProjectMilestones = milestones,
                    MilestoneId = currentMilestoneId,
                    MilestoneName = currentMilestoneName
                };
                return json;
            }
            return null;
        }

        public async Task<bool> DeleteChecklistItem(DeleteChecklistItemModel model, ClaimsPrincipal user)
        {
            Console.WriteLine($"Deleting checklistitem {model.ChecklistItemId}");
            // Grab checklist item from database
            var checklistItem = m_Database.ChecklistItems.Where(item => item.Id == model.ChecklistItemId).FirstOrDefault();
            if (checklistItem == null)
            {
                return false;
            }

            await m_AuditService.InsertLogAsync(AuditLogCategory.Checklist, user, AuditLogType.Deleted, checklistItem.ChecklistId);
            m_Database.ChecklistItems.Remove(checklistItem);
            await m_Database.SaveChangesAsync();
            return true;
        }

        public async Task<CardChecklist> AddChecklist(AddChecklistModel model, ClaimsPrincipal user)
        {
            Console.WriteLine($"Adding checklist {model.Name} to card {model.Id}");
            // Add checklist to card
            CardChecklist checklist = new CardChecklist();
            checklist.ChecklistCard = model.Id;
            checklist.ChecklistName = model.Name;

            await m_Database.CardChecklists.AddAsync(checklist);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Checklist, user, AuditLogType.Added, checklist.Id);
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

        internal async Task DeleteMilestoneAsync(DeleteMilestoneModel model, ClaimsPrincipal user)
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Milestone, user, AuditLogType.Modified, milestone.Id);
        }

        public async Task<ActionResult<GetToggleMilestoneIsClosedModel>> ToggleMilestoneIsClosedAsync(ToggleMilestoneIsClosedModel model, ClaimsPrincipal user)
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Milestone, user, AuditLogType.Added, milestone.);
            return closedModel;
        }

        public async Task<bool> DeleteChecklist(DeleteChecklistModel model, ClaimsPrincipal user)
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Checklist, user, AuditLogType.Deleted, checklist.Id);
            m_Database.CardChecklists.Remove(checklist);

            // Save changes
            await m_Database.SaveChangesAsync();
            return true;
        }

        internal async Task EditMilestoneAsync(EditMilestoneModel model, ClaimsPrincipal user)
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Milestone, user, AuditLogType.Modified, milestone.Id);
        }

        internal async Task AddMilestoneAsync(AddMilestoneModel model, ClaimsPrincipal user)
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Milestone, user, AuditLogType.AddedMilestone, milestone.Id);
        }

        public async Task<bool> ConvertChecklistItemToCard(ConvertChecklistItemModel model, ClaimsPrincipal user)
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
            await AddCard(addCard, user);

            // Delete checklist item
            m_Database.ChecklistItems.Remove(checklistItem);
            await m_Database.SaveChangesAsync();

            return true;
        }

        public async Task EditChecklistItemTickedState(EditChecklistItemTickedStateModel model, ClaimsPrincipal user)
        {
            Console.WriteLine($"Editing checklistitem {model.ChecklistItemId} tick state to {model.TickedState}");
            var checklistItem = m_Database.ChecklistItems.Where(item => item.Id == model.ChecklistItemId).FirstOrDefault();
            if (checklistItem != null)
            {
                checklistItem.ItemTicked = model.TickedState;
                await m_AuditService.InsertLogAsync(AuditLogCategory.Checklist, user, AuditLogType.ModifiedTickState, checklistItem.ChecklistId.ToString());
                await m_Database.SaveChangesAsync();
            }
        }

        public async Task<ChecklistItem> AddChecklistItem(AddChecklistItemModel model, ClaimsPrincipal user)
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

            await m_Database.ChecklistItems.AddAsync(checklistItem);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Checklist, user, AuditLogType.Added, model.ChecklistId);
            return checklistItem;
        }

        public async Task EditColumnTitle(EditColumnNameModel model, ClaimsPrincipal user)
        {
            var column = m_Database.Columns.Where(column => column.Id == model.ColumnId).FirstOrDefault();
            if (column != null)
            {
                column.Title = model.ColumnText;
                await m_AuditService.InsertLogAsync(AuditLogCategory.Column, user, AuditLogType.ModifiedName, model.ColumnId.ToString());
                await m_Database.SaveChangesAsync();
            }
        }

        public ApplicationUser? GetProjectOwner(Project project)
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

        public async Task<AddMemberResponse> AddMember(AddMemberModel model)
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
                        maxMembers = int.MaxValue;
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

            var projOwner = await m_UserManager.FindByIdAsync(model.UserId.ToString());

            await m_AuditService.InsertLogAsync(AuditLogCategory.Project, projOwner, AuditLogType.AddedMember, model.ProjectId);
            await m_Database.ProjectsAccess.AddAsync(access);
            await m_Database.SaveChangesAsync();
            return AddMemberResponse.Success;
        }

        public async Task EditCardAsync(EditCardModel model, ClaimsPrincipal user)
        {
            Console.WriteLine($"Editing card {model.CardId}");
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
                if(Guid.TryParse(model.AssigneeId, out Guid result))
                {
                    card.CardAssignee = result;
                }
                card.CardMilestone = model.MilestoneId;

                var appUser = await m_UserManager.GetUserAsync(user);
                await m_AuditService.InsertLogAsync(AuditLogCategory.Card, appUser, AuditLogType.Modified, card.Id.ToString());
                await m_Database.SaveChangesAsync();
                Console.WriteLine($"Saved card {model.CardId}");
            }
        }
        

        public async Task LeaveProject(LeaveProjectModel model)
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

            var user = await m_UserManager.FindByIdAsync(model.UserId.ToString());
            await m_AuditService.InsertLogAsync(AuditLogCategory.Project, user, AuditLogType.Left, model.ProjectId.ToString());
            Console.WriteLine($"SECURITY: User {model.UserId} left project {model.ProjectId}");
            m_Database.ProjectsAccess.Remove(access);
            await m_Database.SaveChangesAsync();
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

        public async Task AddCard(AddCardModel model, ClaimsPrincipal user)
        {
            Console.WriteLine($"Adding card {model.Name} to project {model.ProjectId}");
            var card = new Card();
            card.ColumnId = model.ColumnId;
            card.CardName = model.Name;
            await m_Database.Cards.AddAsync(card);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.Added, card.Id);
        }
    }
}
