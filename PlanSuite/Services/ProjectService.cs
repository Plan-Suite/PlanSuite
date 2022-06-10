using Microsoft.AspNetCore.Identity;
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
                    Members = members
                };
                return json;
            }
            return null;
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
    }
}
