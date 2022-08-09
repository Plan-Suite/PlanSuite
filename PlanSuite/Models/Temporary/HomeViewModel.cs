﻿using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class HomeViewModel : BaseViewModel
    {
        public CreateProjectModel CreateProject { get; set; } = new CreateProjectModel();
        public CreateOrganisationModel CreateOrganisation { get; set; } = new CreateOrganisationModel();
        public List<Project> OwnedProjects { get; set; } = new List<Project>();
        public Dictionary<int, Organisation> OrganisationMap { get; set; } = new Dictionary<int, Organisation>();
        public List<Project> MemberProjects { get; set; } = new List<Project>();
        public EditProjectModel EditProject { get; set; } = new EditProjectModel();
        public DeleteProjectModel DeleteProject { get; set; } = new DeleteProjectModel();

        public class CreateProjectModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
        }

        public class EditProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            [BindProperty]
            public DateTime? DueDate { get; set; }
        }

        public class DeleteProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
