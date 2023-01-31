using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Models.Temporary.Api;
using PlanSuite.Services;
using PlanSuite.Utility;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly HomeService m_HomeService;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly ILogger<HomeController> m_Logger;

        public HomeController(HomeService homeService, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
        {
            m_HomeService = homeService;
            m_UserManager = userManager;
            m_Logger = logger;
        }

        /// <summary>
        /// GetIndexAsync will be called by the future mobile apps
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpPost("GetIndex")]
        public async Task<string> GetIndexAsync(GetIndexModel getIndex)
        {
            HomeViewModel viewModel = new HomeViewModel();
            m_Logger.LogInformation($"GetIndexAsync start for {getIndex.UserId}");
            var user = await m_UserManager.FindByGuidAsync(getIndex.UserId);
            if (user != null)
            {
                m_Logger.LogInformation($"Getting view model for index for {user.FullName} (org: {getIndex.OrganisationId})");
                viewModel = await m_HomeService.GetHomeViewModelAsync(user, getIndex.OrganisationId);
            }
            return JsonUtility.ToJson(viewModel, true);
        }
    }
}
