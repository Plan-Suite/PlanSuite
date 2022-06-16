namespace PlanSuite.Utility
{
    public static class CommonCookies
    {
        private const string UserLang = "user_lang";

        public static void ApplyCommonCookies(HttpContext httpContext)
        {
            if (!httpContext.Request.Cookies.ContainsKey(UserLang))
            {
                httpContext.Response.Cookies.Append(UserLang, "En-Gb");
            }
        }
    }
}
