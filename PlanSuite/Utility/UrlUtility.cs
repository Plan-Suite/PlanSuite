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
    }
}
