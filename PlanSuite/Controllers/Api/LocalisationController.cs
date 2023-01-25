using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using System.Collections;
using System.Globalization;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class LocalisationController : ControllerBase
    {
        private readonly LocalisationService m_LocalisationService;

        public LocalisationController(LocalisationService localisationService)
        {
            m_LocalisationService = localisationService;
        }

        [HttpGet("GetStrings")]
        public ActionResult<LocalisationResponse> GetStrings(string userLang)
        {
            LocalisationResponse response = new LocalisationResponse();
            response.Data = new Dictionary<string, string>();
            if(userLang.Equals("En-Gb", StringComparison.OrdinalIgnoreCase))
            {
                var resSet = m_LocalisationService.LanguageResources[Enums.Language.English].GetResourceSet(CultureInfo.CurrentCulture, false, false);
                foreach (DictionaryEntry entry in resSet)
                {
                    response.Data.Add(entry.Key.ToString(), entry.Value.ToString());
                }
            }
            return response;
        }
    }
}
