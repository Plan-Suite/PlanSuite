namespace PlanSuite.Enums
{
    /// <summary>
    /// Audit Log Category, determines where each log will be shown.
    /// </summary>
    public enum AuditLogCategory
    {
        Card,
        Project,
        Organisation,
        Column,
        Checklist
    }

    /// <summary>
    /// Audit Log Type, determines what the message of each log will be.
    /// </summary>
    public enum AuditLogType
    {
        Created,
        ModifiedName,
        ModifiedDescription,
        Moved,
        Deleted,
        ModifiedTickState,
        Added,
        Left,
        ModifiedCard,
        AddedMember,
        MakeAdmin,
        RemovedMember
    }
}
