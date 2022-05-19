namespace PlanSuite.Utility
{
    public static class MonthUtil
    {
        public static DateTime GetPreviousMonth(bool returnLastDayOfMonth)
        {
            DateTime firstDayOfThisMonth = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            DateTime lastDayOfLastMonth = firstDayOfThisMonth.AddDays(-1);
            if (returnLastDayOfMonth) return lastDayOfLastMonth;
            return firstDayOfThisMonth.AddMonths(-1);
        }
    }
}
