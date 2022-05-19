namespace PlanSuite.Models.Temporary
{
    public class SaveUserModel
    {
        public string Id { get; set; }
        public string? NewName { get; set; }
        public string? NewEmail { get; set; }
    }

    public class SendPasswordResetModel
    {
        public string Id { get; set; }
    }

    public class GiveAdminModel
    {
        public string Id { get; set; }
    }
}
