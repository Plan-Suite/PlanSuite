using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Controllers;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using System.Security.Claims;

namespace PlanSuite.Services
{
    public class HomeService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ILogger<HomeService> m_Logger;
        private readonly ProjectService m_ProjectService;

        public HomeService(ApplicationDbContext database, UserManager<ApplicationUser> userManager, ILogger<HomeService> logger, ProjectService projectService)
        {
            m_Database = database;
            m_UserManager = userManager;
            m_Logger = logger;
            m_ProjectService = projectService;
        }

        public async Task<HomeViewModel> GetHomeViewModelAsync(ApplicationUser user, int orgId)
        {
            HomeViewModel viewModel = new HomeViewModel();
            viewModel.DueTasks = new List<Models.Persistent.Card>();
            viewModel.OwnedProjects = new List<HomeViewModel.ProjectModel>();
            Guid userId = user.UserId;
            viewModel.CreateOrganisation.OwnerId = userId;

            if (orgId >= 1)
            {
                m_Logger.LogInformation($"Grabbing projects for organisation {orgId} for user {userId}");

                // Validate if they are actually in said organisation.

                var orgMember = m_Database.OrganizationsMembership.Where(member =>
                    member.UserId == userId &&
                    member.OrganisationId == orgId &&
                    member.Role >= ProjectRole.User).FirstOrDefault();
                if (orgMember == null)
                {
                    m_Logger.LogWarning($"User is not a member of organisation {orgId}");
                    return null;
                }

                var organisation = m_Database.Organizations.Where(org => org.Id == orgId).FirstOrDefault();
                if (organisation == null)
                {
                    m_Logger.LogWarning($"Organisation {orgId} does not exist");
                    return null;
                }

                viewModel.Organisations.Add(new ItemList()
                {
                    Name = organisation.Name,
                    Value = organisation.Id
                });

                // Show organisation projects
                m_Logger.LogInformation($"Grabbing organisation projects for organisation {orgId} for user {userId}");
                var organisationProjects = m_Database.Projects.Where(p => p.OrganisationId == orgId).ToList();
                if (organisationProjects != null && organisationProjects.Count > 0)
                {
                    foreach (var project in organisationProjects)
                    {
                        HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                        {
                            Id = project.Id,
                            Name = project.Name,
                            Description = project.Description,
                            CreatedDate = project.CreatedDate,
                            DueDate = project.DueDate,
                            Client = project.Client,
                            Budget = project.Budget,
                            ProjectBudgetType = (int)project.BudgetType,
                            BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                            ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                            OrganisationId = organisation.Id,
                            OrganisationName = organisation.Name,
                            ProjectComplete = project.ProjectCompleted
                        };
                        viewModel.MemberProjects.Add(model);
                    }
                }
                m_Logger.LogInformation($"Added {organisationProjects.Count} projects to user {userId} viewModel");

                viewModel.ViewingOrganisation = organisation;
                viewModel.CurrentOrganisationMembership = orgMember;
                viewModel.CreateProject.OrganisationId = orgId;

                return viewModel;
            }

            m_Logger.LogInformation($"Grabbing projects for user {userId}");
            // Get projects where user is owner
            var ownedProjects = m_Database.Projects.Where(p => p.OwnerId == userId && p.OrganisationId < 1).ToList();
            if (ownedProjects != null && ownedProjects.Count > 0)
            {
                m_Logger.LogInformation($"Grabbing {ownedProjects.Count} owned projects for user {userId}");
                foreach (var project in ownedProjects)
                {
                    Organisation organisation = null;
                    if (project.OrganisationId > 0)
                    {
                        organisation = await m_Database.Organizations.Where(org => org.Id == project.OrganisationId).FirstOrDefaultAsync();
                    }

                    HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        CreatedDate = project.CreatedDate,
                        DueDate = project.DueDate,
                        Client = project.Client,
                        Budget = project.Budget,
                        ProjectBudgetType = (int)project.BudgetType,
                        BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                        ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                        ProjectComplete = project.ProjectCompleted
                    };

                    if (organisation != null)
                    {
                        model.OrganisationId = organisation.Id;
                        model.OrganisationName = organisation.Name;
                    }
                    viewModel.OwnedProjects.Add(model);
                }

                m_Logger.LogInformation($"Grabbing unowned due tasks for user {userId}");
                foreach (var project in ownedProjects)
                {
                    var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                    if (columns != null && columns.Count > 0)
                    {
                        foreach (var column in columns)
                        {
                            var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                            if (cards != null && cards.Count > 0)
                            {
                                foreach (var card in cards)
                                {
                                    if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                    {
                                        viewModel.DueTasks.Add(card);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Get projects where user is member
            var projectAccesses = m_Database.ProjectsAccess.Where(access => access.UserId == userId).ToList();
            if (projectAccesses != null && projectAccesses.Count > 0)
            {
                m_Logger.LogInformation($"Grabbing {projectAccesses.Count} member projects for user {userId}");
                foreach (var access in projectAccesses)
                {
                    var project = m_Database.Projects.Where(project => project.Id == access.ProjectId && access.ProjectRole >= ProjectRole.User).FirstOrDefault();
                    if (project != null)
                    {
                        Organisation organisation = null;
                        if (project.OrganisationId > 0)
                        {
                            organisation = await m_Database.Organizations.Where(org => org.Id == project.OrganisationId).FirstOrDefaultAsync();
                        }

                        HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                        {
                            Id = project.Id,
                            Name = project.Name,
                            Description = project.Description,
                            CreatedDate = project.CreatedDate,
                            DueDate = project.DueDate,
                            Client = project.Client,
                            Budget = project.Budget,
                            ProjectBudgetType = (int)project.BudgetType,
                            BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                            ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                            ProjectComplete = project.ProjectCompleted
                        };

                        if (organisation != null)
                        {
                            model.OrganisationId = organisation.Id;
                            model.OrganisationName = organisation.Name;
                        }

                        viewModel.MemberProjects.Add(model);

                        m_Logger.LogInformation($"Grabbing due tasks for user {userId} for project {project.Id}");
                        var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                        if (columns != null && columns.Count > 0)
                        {
                            foreach (var column in columns)
                            {
                                var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                                if (cards != null && cards.Count > 0)
                                {
                                    foreach (var card in cards)
                                    {
                                        if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                        {
                                            viewModel.DueTasks.Add(card);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // get all organisations user is member of and then return said organisation owned projects projects
            var organisationMemberships = m_Database.OrganizationsMembership.Where(member => member.UserId == userId).ToList();
            if (organisationMemberships != null && organisationMemberships.Count > 0)
            {
                m_Logger.LogInformation($"Grabbing organisation projects for {organisationMemberships.Count} organisations for user {userId}");
                foreach (var organisationMemership in organisationMemberships)
                {
                    int organisationId = organisationMemership.OrganisationId;
                    var organisation = m_Database.Organizations.Where(o => o.Id == organisationId).FirstOrDefault();
                    if (organisation != null)
                    {
                        // Confirm if user is actually a member of said organisation
                        var organisationMembership = m_Database.OrganizationsMembership.Where(member => member.OrganisationId == organisationId && member.UserId == userId && member.Role >= ProjectRole.User).FirstOrDefault();
                        if (organisationMembership != null)
                        {
                            if (organisationMembership.Role >= ProjectRole.Admin)
                            {
                                viewModel.Organisations.Add(new ItemList()
                                {
                                    Name = organisation.Name,
                                    Value = organisation.Id
                                });
                            }
                            var organisationProjects = m_Database.Projects.Where(p => p.OrganisationId == organisationId).ToList();
                            if (organisationProjects != null && organisationProjects.Count > 0)
                            {
                                foreach (var project in organisationProjects)
                                {
                                    m_Logger.LogInformation($"Grabbing organisation due tasks for user {userId} for organisation {organisation.Id} project {project.Id}");
                                    var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                                    if (columns != null && columns.Count > 0)
                                    {
                                        foreach (var column in columns)
                                        {
                                            var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                                            if (cards != null && cards.Count > 0)
                                            {
                                                foreach (var card in cards)
                                                {
                                                    if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                                    {
                                                        viewModel.DueTasks.Add(card);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                                    {
                                        Id = project.Id,
                                        Name = project.Name,
                                        Description = project.Description,
                                        CreatedDate = project.CreatedDate,
                                        DueDate = project.DueDate,
                                        Client = project.Client,
                                        Budget = project.Budget,
                                        ProjectBudgetType = (int)project.BudgetType,
                                        BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                                        ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                                        OrganisationId = organisation.Id,
                                        OrganisationName = organisation.Name,
                                        ProjectComplete = project.ProjectCompleted
                                    };

                                    viewModel.MemberProjects.Add(model);
                                }
                            }
                        }
                    }
                }
            }
            return viewModel;
        }

        public async Task<List<Project>> GetOrganisationProjects(ApplicationUser user, int orgId)
        {
            m_Logger.LogInformation($"Grabbing organisation projects for organisation {orgId} for user {user.UserId}");
            return await m_Database.Projects.Where(p => p.OrganisationId == orgId).ToListAsync();
        }

        public async Task<List<Project>> GetUserProjects(ApplicationUser user, int orgId)
        {
            Guid userId = user.UserId;



            return await m_Database.Projects.Where(p => p.OrganisationId == orgId).ToListAsync();
        }
    }
}
