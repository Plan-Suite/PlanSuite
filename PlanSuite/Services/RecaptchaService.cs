using PlanSuite.Interfaces;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;

namespace PlanSuite.Services
{
    public class RecaptchaService : ICaptchaService
    {
        private readonly IConfiguration m_Configuration;

        public RecaptchaService(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public async Task<CaptchaResponse> Verify(string token)
        {
            CaptchaData data = new CaptchaData
            {
                Response = token,
                Secret = m_Configuration["CaptchaKey"]
            };

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={data.Secret}&response={data.Response}");
            var captchaResponse = JsonUtility.FromJson<CaptchaResponse>(response);
            return captchaResponse;
        }
    }
}
