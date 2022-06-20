using PlanSuite.Enums;

namespace PlanSuite.Models.Temporary
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public ErrorCode ErrorCode { get; set; }
    }
}