namespace PlanSuite.Enums
{
    public enum ProjectRole
    {
        None,
        User,
        Admin,
        Owner
    };

    public enum AddMemberResponse
    {
        Success = 0,
        ServerError,
        IncorrectRoles,
        NoUser,
        AlreadyHasAccess
    }
}
