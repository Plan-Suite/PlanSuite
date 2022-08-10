using Microsoft.AspNetCore.Mvc;
using PlanSuite.Enums;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers
{
    public class OrganisationController : Controller
    {
        private readonly OrganisationService m_OrganisationService;
        private readonly ILogger<OrganisationController> m_Logger;

        public OrganisationController(OrganisationService organisationService, ILogger<OrganisationController> logger)
        {
            m_OrganisationService = organisationService;
            m_Logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateOrganisationModel createOrganisation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("MODEL_STATE_NOT_VALID");
            }

            m_Logger.LogInformation($"CreateOrganisation: {createOrganisation.Name} {createOrganisation.Description} {createOrganisation.OwnerId}");
            OrganisationErrorCode errorCode = await m_OrganisationService.OnCreateOrganisation(createOrganisation);

            if(errorCode != OrganisationErrorCode.Success)
            {
                return BadRequest(new
                {
                    ErrorCode = errorCode
                });
            }

            return Ok(new
            {
                ErrorCode = errorCode
            });
        }
    }
}
