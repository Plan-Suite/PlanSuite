using Microsoft.AspNetCore.Mvc;
using PlanSuite.Enums;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class OrganisationController : Controller
    {
        private readonly OrganisationService m_OrganisationService;

        public OrganisationController(OrganisationService organisationService)
        {
            m_OrganisationService = organisationService;
        }

        [HttpPost("CreateOrganisation")]
        public async Task<IActionResult> OnCreateOrganisation([FromBody] CreateOrganisationModel model)
        {
            Console.WriteLine($"CreateOrganisation: {model.Name} {model.Description} {model.OwnerId}");
            OrganisationErrorCode errorCode = await m_OrganisationService.OnCreateOrganisation(model);

            return Ok(new
            {
                ErrorCode = errorCode
            });
        }
    }
}
