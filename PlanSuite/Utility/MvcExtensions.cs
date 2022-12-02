using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.Security.Claims;

namespace PlanSuite.Utility
{
    public static class MvcExtensions
    {
        public static string ActiveClass(this IHtmlHelper htmlHelper, string controllers = null, string actions = null, string cssClass = "nav-active")
        {
            var currentController = htmlHelper?.ViewContext.RouteData.Values["controller"] as string;
            if(string.IsNullOrEmpty(currentController))
            {
                currentController = htmlHelper?.ViewContext.RouteData.Values["area"] as string;
            }

            var currentAction = htmlHelper?.ViewContext.RouteData.Values["action"] as string;
            if (string.IsNullOrEmpty(currentController))
            {
                currentAction = htmlHelper?.ViewContext.RouteData.Values["page"] as string;
            }

            var acceptedControllers = (controllers ?? currentController ?? "").Split(',');
            var acceptedActions = (actions ?? currentAction ?? "").Split(',');

            return acceptedControllers.Contains(currentController) && acceptedActions.Contains(currentAction)
                ? cssClass
                : "";
        }
    }
}
