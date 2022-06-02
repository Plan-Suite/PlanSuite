using PlanSuite.Data;
using PlanSuite.Enums;
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
