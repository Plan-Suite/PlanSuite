using crypto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;
using System.Globalization;
using System.Security.Claims;

namespace PlanSuite.Services
{
    public class ProjectService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly AuditService m_AuditService;
        private readonly IEmailSender m_EmailSender;
        private readonly ILogger<ProjectService> m_Logger;
        private readonly SecurityService m_Security;

        public ProjectService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, AuditService auditService, IEmailSender emailSender, ILogger<ProjectService> logger, SecurityService security)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
            m_AuditService = auditService;
            m_EmailSender = emailSender;
            m_Logger = logger;
            m_Security = security;
        }

        public PaymentTier GetProjectTier(int projectId)
        {
            var project = m_Database.Projects.Where(proj => proj.Id == projectId).FirstOrDefault();
            if (project == null)
            {
                return PaymentTier.Free;
            }

            return GetProjectTier(project);
        }

        public PaymentTier GetProjectTier(Project project)
        {
            var owner = GetProjectOwner(project);
            if (owner == null)
            {
                return PaymentTier.Free;
            }

            return owner.PaymentTier;
        }

        public decimal GetUsedBudget(int projectId)
        {
            var project = m_Database.Projects.Where(proj => proj.Id == projectId).FirstOrDefault();
            if(project == null)
            {
                return 0m;
            }

            var columns = m_Database.Columns.Where(column => column.ProjectId == project.Id).ToList();
            if (columns == null)
            {
                return 0m;
            }

            decimal budget = 0m;
            foreach (var column in columns)
            {
                foreach (var card in m_Database.Cards.Where(card => card.ColumnId == column.Id))
                {
                    budget += card.Budget;
                }
            }
            return budget;
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
                    assignee = user.FullName;
                    assigneeId = user.Id;
                }

                string createdBy = "UNKNOWN";
                string createdById = "0";
                ApplicationUser creator = await m_UserManager.FindByIdAsync(card.CreatedBy.ToString());
                if (creator != null)
                {
                    createdBy = creator.FullName;
                    createdById = creator.Id;
                }

                uint unixTime = 0;
                if(card.CardDueDate != null)
                {
                    unixTime = (uint)new DateTimeOffset((DateTime)card.CardDueDate).ToUnixTimeSeconds();
                }

                uint startTime = 0;
                if (card.CardStartDate != null)
                {
                    startTime = (uint)new DateTimeOffset((DateTime)card.CardStartDate).ToUnixTimeSeconds();
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
                    members.Add(Guid.Parse(owner.Id), owner.FullName);
                }

                var projMembers = GetProjectUsers(projId);
                foreach(var member in projMembers)
                {
                    if(!members.ContainsKey(member.Key))
                    {
                        members.Add(member.Key, member.Value);
                    }
                }

                decimal budget = 0m;
                string budgetUnit = string.Empty;
                ProjectBudgetType budgetType = ProjectBudgetType.None;
                if(GetProjectTier(project.Id) >= PaymentTier.Plus)
                {
                    budget = card.Budget;
                    if (budget > 0m)
                    {
                        budgetUnit = project.BudgetMonetaryUnit;
                        budgetType = project.BudgetType;
                    }
                    Console.WriteLine($"Budget info: {budgetUnit}{budget} {budgetType}");
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

                // get last 5 card audit logs
                List<AuditLogJsonModel> auditLogs = new List<AuditLogJsonModel>();
                var cardAuditLogs = m_Database.AuditLogs.Where(m => m.LogCategory == AuditLogCategory.Card && m.TargetID == card.Id.ToString()).ToList().TakeLast(5);
                foreach(var auditLog in cardAuditLogs)
                {
                    var auditUser = await m_UserManager.FindByIdAsync(auditLog.UserID.ToString());
                    AuditLogJsonModel auditLogModel = new AuditLogJsonModel();
                    auditLogModel.Message = await m_AuditService.AuditLogToHumanReadable(auditLog);
                    auditLogModel.Created = UrlUtility.TimestampToLastUpdated(auditLog.Timestamp);
                    auditLogModel.Username = auditUser.FullName;
                    auditLogs.Add(auditLogModel);
                }

                List<EditCardNameModel> availableTasks = new List<EditCardNameModel>();
                var tasksOnProjects = m_Database.Cards.Where(m => m.Id == projId).ToList();
                foreach (var task in tasksOnProjects)
                {
                    EditCardNameModel taskOnProject = new EditCardNameModel();
                    taskOnProject.Name = task.CardName;
                    taskOnProject.CardId = task.Id;
                    availableTasks.Add(taskOnProject);
                }

                // return
                GetCardReturnJson json = new GetCardReturnJson()
                {
                    Name = cardName,
                    MarkdownContent = Markdown.Parse(cardDesc),
                    RawContent = cardDesc,
                    UnixTimestamp = unixTime,
                    StartDate = startTime,
                    AssigneeName = assignee,
                    AssigneeId = assigneeId,
                    Priority = card.CardPriority,
                    Members = members,
                    CardChecklists = cardChecklists,
                    ChecklistItems = items,
                    ProjectMilestones = milestones,
                    MilestoneId = currentMilestoneId,
                    MilestoneName = currentMilestoneName,
                    AuditLogs = auditLogs,
                    CreatedBy = createdBy,
                    Budget = budget,
                    BudgetType = budgetType,
                    BudgetUnit = budgetUnit,
                    ProjectName = project.Name,
                    ProjectId = project.Id,
                    AvailableTasks = availableTasks
                };
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(json));
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

        public ActionResult<ChartViewModel> GetChartData(int id)
        {
            int lastCol = 0;
            ChartViewModel model = new ChartViewModel();
            model.Project = m_Database.Projects.Where(project => project.Id == id).First();
            if(model.Project == null)
            {
                return model;
            }

            model.Dataset = new List<ChartViewModel.ChartDataset>();
            foreach(var column in m_Database.Columns.Where(col => col.ProjectId == id).ToList())
            {
                int colId = column.Id;
                int cardCount = 0;
                foreach(var card in m_Database.Cards.Where(card => card.ColumnId == colId).ToList())
                {
                    cardCount++;
                }
                ChartViewModel.ChartDataset dataset = new ChartViewModel.ChartDataset(column.Title, cardCount, lastCol);
                lastCol++;
                if(lastCol > ChartViewModel.ValidColours.Length)
                {
                    lastCol = 0;
                }
                model.Dataset.Add(dataset);
            }
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
            await m_AuditService.InsertLogAsync(AuditLogCategory.Milestone, user, AuditLogType.Closed, milestone.Id);
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
                        members.Add(Guid.Parse(projMember.Id), projMember.FullName);
                    }
                }
            }
            return members;
        }

        public async Task<AddMemberResponse> AddMember(ProjectViewModel.AddMemberModel model)
        {
            Console.WriteLine($"AddMember start for Email:{model.Email} UserId:{model.UserId} ProjectId: {model.ProjectId}");
            var project = m_Database.Projects.Where(project => project.Id == model.ProjectId).FirstOrDefault();
            if (project == null)
            {
                return AddMemberResponse.ServerError;
            }

            var projectOwner = await m_UserManager.FindByIdAsync(project.OwnerId.ToString());
            if(projectOwner == null)
            {
                return AddMemberResponse.NoUser;
            }

            var inviter = await m_UserManager.FindByIdAsync(model.SenderId.ToString());
            if (inviter == null)
            {
                return AddMemberResponse.ServerError;
            }

            int maxUsers = int.MaxValue;
            if(projectOwner.PaymentTier < PaymentTier.Plus)
            {
                maxUsers = 20;
            }

            int currentMembers = m_Database.ProjectsAccess.Where(access => access.ProjectId == model.ProjectId).Count();
            if(currentMembers > maxUsers)
            {
                return AddMemberResponse.IncorrectTier;
            }

            if(!string.IsNullOrEmpty(model.Email))
            {
                Console.WriteLine($"Get user by email");
                // Get user by email
                var user = await m_UserManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    if(UrlUtility.IsValidEmail(model.Email))
                    {
                        Console.WriteLine($"invite {model.Email}");
                        Invitation invitation = new Invitation();
                        invitation.Expiry = DateTime.Now.AddDays(7);
                        invitation.Code = RandomGenerator.GetUniqueKey(15);
                        invitation.Email = model.Email;
                        invitation.Project = project.Id;

                        await m_Database.Invitations.AddAsync(invitation);
                        await m_Database.SaveChangesAsync();

                        string inviteLink = $"https://plan-suite.com/invite/{invitation.Code}";
                        Console.WriteLine($"Generated invite link for {invitation.Email}: {inviteLink}");

                        string mailMessage = "Hello!<br><br>" +
                            $"You have been invited by {inviter.FullName} to work on {project.Name} at Plan Suite.<br><br>" +
                            $"Plan Suite is a professional project management website for teams and individuals alike that helps to make projects easier to manage.<br><br>" +
                            $"<a href=\"{inviteLink}\">Accept Invitiation</a><br><br> - This invite will expire in 7 days if not accepted." +
                            $"Regards,<br>Plan Suite.";

                        await m_EmailSender.SendEmailAsync(model.Email, "You have been invited to join Plan Suite", mailMessage);
                    }
                }
                else
                {
                    bool isActive = m_Database.ProjectsAccess.Where(access => access.UserId == Guid.Parse(user.Id) && access.ProjectId == model.ProjectId).FirstOrDefault() != null;
                    if(isActive)
                    {
                        return AddMemberResponse.AlreadyHasAccess;
                    }
                    Console.WriteLine($"add {user.FullName} to {project.Name}");

                    await CreateProjectAccess(user, model.ProjectId);
                }
                return AddMemberResponse.Success;
            }

            if (model.UserId != Guid.Empty)
            {
                var user = await m_UserManager.FindByIdAsync(model.UserId.ToString());
                if(user == null)
                {
                    return AddMemberResponse.NoUser;
                }

                await CreateProjectAccess(user, project.Id);
                return AddMemberResponse.Success;
            }
            return AddMemberResponse.NoUser;
        }

        public async Task CreateProjectAccess(ApplicationUser user, int projectId)
        {
            var projectAccess = new ProjectAccess();
            projectAccess.UserId = Guid.Parse(user.Id);
            projectAccess.ProjectId = projectId;
            projectAccess.AccessSince = DateTime.Now;
            projectAccess.ProjectRole = ProjectRole.User;
            await m_Database.ProjectsAccess.AddAsync(projectAccess);
            await m_Database.SaveChangesAsync();
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
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(model));
                card.CardDueDate = dueDate;
                card.CardPriority = (Priority)model.Priority;
                if(Guid.TryParse(model.AssigneeId, out Guid result))
                {
                    card.CardAssignee = result;
                }
                card.CardMilestone = model.MilestoneId;
                card.Budget = model.Budget;

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
            await m_Security.WriteLogAsync(user, LogAction.Update, "Project", $"User {model.UserId} left project {model.ProjectId}");
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
                cardMembers.CardOwner = owner.FullName;
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
                        Console.WriteLine($"adminUser = {adminUser.FullName}");
                        if (!cardMembers.CardAdmins.Contains(adminUser.FullName))
                        {
                            cardMembers.CardAdmins.Add(adminUser.FullName);
                        }
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
                        Console.WriteLine($"regularUser = {regularUser.FullName}");
                        if (!cardMembers.CardMembers.Contains(regularUser.FullName))
                        {
                            cardMembers.CardMembers.Add(regularUser.FullName);
                        }
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

        [Obsolete]
        public async Task AddCard(AddCardModel model, ClaimsPrincipal user)
        {
            var appUser = await m_UserManager.GetUserAsync(user);
            if(appUser == null)
            {
                return;
            }

            Console.WriteLine($"Adding card {model.Name} to project {model.ProjectId}");
            var card = new Card();
            card.ColumnId = model.ColumnId;
            card.CardName = model.Name;
            card.CardStartDate = DateTime.Now;
            card.CreatedBy = Guid.Parse(appUser.Id);
            await m_Database.Cards.AddAsync(card);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.Added, card.Id);
        }

        internal async Task AddTask(ProjectViewModel.AddTaskModel model, ClaimsPrincipal user)
        {
            var appUser = await m_UserManager.GetUserAsync(user);
            if (appUser == null)
            {
                return;
            }

            Console.WriteLine($"Adding task {model.Name} to column {model.ColumnId}");
            var card = new Card();
            card.ColumnId = model.ColumnId;
            card.CardStartDate = DateTime.Now;
            card.CreatedBy = Guid.Parse(appUser.Id);

            card.CardName = model.Name;
            card.CardDescription = model.Content;

            card.CardAssignee = Guid.Empty;
            if(model.Assignee != null && model.Assignee != Guid.Empty)
            {
                card.CardAssignee = (Guid)model.Assignee;
            }

            card.CardDueDate = model.DueDate;
            card.CardPriority = model.Priority;
            card.CardMilestone = model.MilestoneId;

            await m_Database.Cards.AddAsync(card);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.Added, card.Id);
        }

        internal async Task<int> AddTask(ClaimsPrincipal user, int columnId, string name, string description, DateTime? dueDate)
        {
            var appUser = await m_UserManager.GetUserAsync(user);
            if (appUser == null)
            {
                return 0;
            }

            Console.WriteLine($"Adding task {name} to column {columnId}");
            var card = new Card();
            card.ColumnId = columnId;
            card.CardStartDate = DateTime.Now;
            card.CreatedBy = Guid.Parse(appUser.Id);
            card.CardName = name;
            card.CardDescription = description;
            card.CardDueDate = dueDate;

            await m_Database.Cards.AddAsync(card);
            await m_Database.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Card, user, AuditLogType.Added, card.Id);
            return card.Id;
        }

        public async Task<bool> MarkCompleteAsync(MarkCompleteModel model, ClaimsPrincipal user)
        {
            // Grab checklist item from database
            var project = m_Database.Projects.Where(item => item.Id == model.ProjectId).FirstOrDefault();
            if (project == null)
            {
                return false;
            }

            bool complete = !project.ProjectCompleted;
            m_Logger.LogInformation($"Marking project {model.ProjectId} as complete: {complete}");
            project.ProjectCompleted = complete;
            await m_Database.SaveChangesAsync();

            if(complete)
            {
                await m_AuditService.InsertLogAsync(AuditLogCategory.Project, user, AuditLogType.Closed, project.Id);
            }
            else
            {
                await m_AuditService.InsertLogAsync(AuditLogCategory.Project, user, AuditLogType.Opened, project.Id);
            }
            return complete;
        }

        public async Task<int> AddColumnAsync(int projectId, string name)
        {
            var column = new Column();
            column.ProjectId = projectId;
            column.Title = name;
            await m_Database.Columns.AddAsync(column);
            await m_Database.SaveChangesAsync();
            return column.Id;
        }

        public async Task<List<GetCalendarTasksModel.CalendarTask>> GetCalendarTasksAsync(int id, Guid teamMember, string? start = null, string? end = null)
        {
            m_Logger.LogInformation($"GetCalendarTasks: Checking if project {id} exists");
            var project = await m_Database.Projects.Where(item => item.Id == id).FirstOrDefaultAsync();
            if (project == null)    
            {
                return null;
            }

            m_Logger.LogInformation($"GetCalendarTasks: Getting columns for project {id}");
            var cols = await m_Database.Columns.Where(col => col.ProjectId == project.Id).ToListAsync();
            if(cols == null || cols.Count < 1)
            {
                return null;
            }

            GetCalendarTasksModel tasksModel = new GetCalendarTasksModel();
            tasksModel.Events = new List<GetCalendarTasksModel.CalendarTask>();
            foreach (var col in cols)
            {
                m_Logger.LogInformation($"GetCalendarTasks: Getting tasks for column {col.Id} that have both a start and end date, and have assignee of {teamMember}");
                List<Card> cards;
                if(teamMember == Guid.Empty)
                {
                    cards = await m_Database.Cards.Where(card => card.ColumnId == col.Id).ToListAsync();
                }
                else
                {
                    cards = await m_Database.Cards.Where(card => card.ColumnId == col.Id && card.CardAssignee == teamMember).ToListAsync();
                }

                if (cards == null || cards.Count < 1)
                {
                    continue;
                }

                foreach(var card in cards)
                {
                    if(card.CardStartDate == null || card.CardDueDate == null)
                    {
                        continue;
                    }

                    GetCalendarTasksModel.CalendarTask calendarTask = new GetCalendarTasksModel.CalendarTask();
                    calendarTask.Id = card.Id.ToString();
                    calendarTask.Title = card.CardName;
                    calendarTask.Start = card.CardStartDate?.ToString("yyyy-MM-dd");
                    calendarTask.End = card.CardDueDate?.ToString("yyyy-MM-dd");
                    calendarTask.Completed = card.IsFinished;
                    if(calendarTask.Completed)
                    {
                        // I dont like this one bit, would prefer if we could define this on the client-side
                        calendarTask.BackgroundColor = "rgba(58,95,218, 0.5)";
                    }
                    tasksModel.Events.Add(calendarTask);
                }
            }
            return tasksModel.Events;
        }

        public async Task EditTaskDates(int id, string newStartDate, string newDueDate)
        {
            m_Logger.LogInformation($"Editing task {id} dates: newStartDate: {newStartDate}, newDueDate: {newDueDate}");
            DateTime startDate = DateTime.Parse(newStartDate);
            DateTime dueDate = DateTime.Parse(newDueDate);
            var task = await m_Database.Cards.Where(task => task.Id == id).FirstOrDefaultAsync();
            if(task == null)
            {
                m_Logger.LogError($"Could not find task {id}");
                return;
            }

            task.CardStartDate = startDate;
            task.CardDueDate = dueDate;
            await m_Database.SaveChangesAsync();
            m_Logger.LogInformation($"Finished editing task {id} dates");
        }
    }
}
