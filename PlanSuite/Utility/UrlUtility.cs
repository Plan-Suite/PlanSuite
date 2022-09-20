using Microsoft.AspNetCore.Http.Extensions;

namespace PlanSuite.Utility
{
    public static class UrlUtility
    {
        public static int GetUrlRouteInt(HttpRequest request, string routeKey)
        {
            int routeValue = 0;
            string routeVal = GetUrlRouteValue(UriHelper.GetDisplayUrl(request), routeKey);
            if (!string.IsNullOrEmpty(routeVal))
            {
                int.TryParse(routeVal, out routeValue);
            }
            return routeValue;
        }

        private static string GetUrlRouteValue(string url, string routeKey)
        {
            Console.WriteLine($"GetUrlRouteValue for url {url} with routeKey {routeKey}");
            if (!url.Contains("?"))
            {
                Console.WriteLine($"No route keys in {url}");
                return string.Empty;
            }

            if(url.Contains($"{routeKey}="))
            {
                string routeVal = url.Substring(url.IndexOf($"{routeKey}=") + routeKey.Length + 1);
                if(routeVal.Contains("&"))
                {
                    int index = routeVal.IndexOf("&");
                    routeVal = routeVal.Substring(0, index);
                }
                Console.WriteLine($"GetUrlRouteValue for {routeKey}: {routeVal}");
                return routeVal;
            }
            Console.WriteLine($"No route key for {routeKey} in {url}");
            return string.Empty;
        }

        public static string TimestampToLastUpdated(DateTime timestamp)
        {
            string format = string.Empty;
            DateTime when = timestamp;
            TimeSpan ts = DateTime.Now.Subtract(when);
            if (ts.TotalMinutes < 1)
            {
                format = $"{(int)ts.TotalSeconds} seconds ago";
            }
            else if (ts.TotalHours < 1)
            {
                format = $"{(int)ts.TotalMinutes} minutes ago";
            }
            else if (ts.TotalDays < 1)
            {
                format = $"{(int)ts.TotalHours} hours ago";
            }
            else if (ts.TotalDays < 2)
            {
                format = "yesterday";
            }
            else if (ts.TotalDays < 5)
            {
                format = $"on {when.DayOfWeek}";
            }
            else
            {
                format = $"{(int)ts.TotalDays} days ago";
            }
            return format;
        }
    }
}
