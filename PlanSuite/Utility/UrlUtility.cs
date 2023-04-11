using Microsoft.AspNetCore.Http.Extensions;
using PlanSuite.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace PlanSuite.Utility
{
    public static class UrlUtility
    {
        public static string GetUrlRouteString(HttpRequest request, string routeKey)
        {
            string routeVal = GetUrlRouteValue(UriHelper.GetDisplayUrl(request), routeKey);
            if (!string.IsNullOrEmpty(routeVal))
            {
                return routeVal;
            }
            return routeVal;
        }

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

        public static string StripHtml(string text)
        {
            List<int> openTagIndexes = Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
            List<int> closeTagIndexes = Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();
            if (closeTagIndexes.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                int previousIndex = 0;
                foreach (int closeTagIndex in closeTagIndexes)
                {
                    var openTagsSubset = openTagIndexes.Where(x => x >= previousIndex && x < closeTagIndex);
                    if (openTagsSubset.Count() > 0 && closeTagIndex - openTagsSubset.Max() > 1)
                    {
                        sb.Append(text.Substring(previousIndex, openTagsSubset.Max() - previousIndex));
                    }
                    else
                    {
                        sb.Append(text.Substring(previousIndex, closeTagIndex - previousIndex + 1));
                    }
                    previousIndex = closeTagIndex + 1;
                }
                if (closeTagIndexes.Max() < text.Length)
                {
                    sb.Append(text.Substring(closeTagIndexes.Max() + 1));
                }
                return sb.ToString();
            }
            else
            {
                return text;
            }
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                return new System.Net.Mail.MailAddress(email).Address == email && !email.Trim().EndsWith(".");
            }
            catch
            {
                return false;
            }
        }
    }

    public static class EventUtility
    {
        public static void FireEvent(string eventName)
        {
            Console.WriteLine($"Fired {eventName}");
            // TODO: figure out how to get db context here
        }
    }
}
