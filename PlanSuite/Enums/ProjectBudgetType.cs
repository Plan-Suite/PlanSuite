namespace PlanSuite.Enums
{
    /// <summary>
    /// Project Budget Types
    /// </summary>
    public enum ProjectBudgetType
    {
        /// <summary>
        /// Project will not have a budget
        /// </summary>
        None,

        /// <summary>
        /// Project will be budgeted by monetary units
        /// </summary>
        Cost,

        /// <summary>
        /// Project will be budgeted by hours
        /// </summary>
        Hours,

        /// <summary>
        /// Project will be budgeted by days
        /// </summary>
        Days
    }
}
