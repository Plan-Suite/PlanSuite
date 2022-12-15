namespace PlanSuite.Enums
{
    /// <summary>
    /// Project/Task Approval Types
    /// </summary>
    public enum ApprovalType
    {
        /// <summary>
        /// Default State for all projects
        /// </summary>
        Pending,

        /// <summary>
        /// Has been approved
        /// </summary>
        Approved,

        /// <summary>
        /// Project manager / admin is requesting changes
        /// </summary>
        RequestingChanges,

        /// <summary>
        /// Project manager / admin has rejected the project/task.
        /// </summary>
        Rejected,
    }
}
