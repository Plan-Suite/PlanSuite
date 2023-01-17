using PlanSuite.Interfaces;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;

namespace PlanSuite.Services
{
    public class RecaptchaService : ICaptchaService
    {
        private readonly IConfiguration m_Configuration;
        private readonly ILogger<RecaptchaService> m_Logger;

        public RecaptchaService(IConfiguration configuration, ILogger<RecaptchaService> logger)
        {
            m_Configuration = configuration;
            m_Logger = logger;
        }

        public async Task<CaptchaResponse> Verify(string token)
        {
            m_Logger.LogInformation($"Verifying token {token}");
            CaptchaData data = new CaptchaData
            {
                Response = token,
                Secret = m_Configuration["CaptchaKey"]
            };

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={data.Secret}&response={data.Response}");
            var captchaResponse = JsonUtility.FromJson<CaptchaResponse>(response);
            m_Logger.LogInformation($"Recaptcha token {token} returned {captchaResponse.Success}");
            return captchaResponse;
        }
    }
}
