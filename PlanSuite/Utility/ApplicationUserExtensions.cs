using Microsoft.AspNetCore.Mvc.Rendering;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Utility
{
    public static class ApplicationUserExtensions
    {
        public static string FormatDate(this ApplicationUser user, DateTime? value)
        {
            string format = "dd/MMM/yyyy";
            // TODO: format date format based on users region

            return value.HasValue ? value.Value.ToString(format) : "N/A";
        }

        public static string FormatMaxBudget(this ApplicationUser user, decimal budget, string budgetUnit, ProjectBudgetType budgetType)
        {
            string decimalFormat = "N";
            // TODO: format decimal format based on users region (i.e. using commas instead of dots as decimal points in certain regions, etc)

            string symbol = string.Empty;
            string budgetName = string.Empty;
            switch (budgetType)
            {
                case ProjectBudgetType.Cost:
                    symbol = budgetUnit;
                    break;
                case ProjectBudgetType.Hours:
                    budgetName = " hours";
                    break;
                case ProjectBudgetType.Days:
                    budgetName = " days";
                    break;
            }
            return $"{symbol}{budget.ToString(decimalFormat)}{budgetName}";
        }

        public static string FormatBudget(this ApplicationUser user, decimal budget, string budgetUnit, ProjectBudgetType budgetType)
        {
            string decimalFormat = "N";
            // TODO: format decimal format based on users region (i.e. using commas instead of dots as decimal points in certain regions, etc)

            string symbol = string.Empty;
            string budgetName = string.Empty;
            switch (budgetType)
            {
                case ProjectBudgetType.Cost:
                    symbol = budgetUnit;
                    break;
            }
            return $"{symbol}{budget.ToString(decimalFormat)}{budgetName}";
        }

        public static List<SelectListItem> GetDropdownItemsFromEnum(this ApplicationUser user, Enum enumType)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var enumValues = Enum.GetValues(enumType.GetType());
            foreach (var value in enumValues)
            {
                var item = new SelectListItem();
                item.Text = value.ToString();
                item.Value = ((int)value).ToString();
                items.Add(item);
            }
            return items.ToList();
        }
    }
}
