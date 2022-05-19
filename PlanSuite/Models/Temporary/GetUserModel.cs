using PlanSuite.Enums;

namespace PlanSuite.Models.Temporary
{
    public class GetUsersModel
    {
        public GetUserModel[] GetUserModels { get;set; }
    }

    public class GetUserModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public PaymentTier PaymentTier { get; set; }
        public string[] Roles { get; set; }
    }
}
