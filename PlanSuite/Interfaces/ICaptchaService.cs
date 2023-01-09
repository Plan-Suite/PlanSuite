using PlanSuite.Models.Temporary;

namespace PlanSuite.Interfaces
{
    public interface ICaptchaService
    {
        Task<CaptchaResponse> Verify(string token);
    }
}
