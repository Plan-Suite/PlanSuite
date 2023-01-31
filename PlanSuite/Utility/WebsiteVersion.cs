namespace PlanSuite.Utility
{
    /// <summary>
    /// Website Version
    /// </summary>
    public static class WebsiteVersion
    {
        public static void Init(string environmentName)
        {
            EnvironmentName = environmentName;
            if (environmentName.Equals("Production"))
            {
                IsProduction = true;
            }
        }

        /// <summary>
        /// Increment this when we add/change major features, UI overhauls or major backend changes
        /// If this gets incremented, then we reset minor and patch to 0
        /// </summary>
        public const int VERSION_MAJOR = 0;

        /// <summary>
        /// Increment this when we add/change minor features, incremental UI changes or backend changes
        /// If this gets incremented, then we reset patch to 0
        /// </summary>
        public const int VERSION_MINOR = 16;

        // Increment this when we fix bugs, update dependencies or other non-noticable features
        public const int VERSION_PATCH = 0;

        public static bool IsProduction { get; private set; }
        public static string EnvironmentName { get; private set; }

        public static string Version
        {
            get
            {
                string version = $"{VERSION_MAJOR}.{VERSION_MINOR}.{VERSION_PATCH}";
                if(!IsProduction)
                {
                    version += $"-{EnvironmentName.ToLower()}";
                }
                return version;
            }
        }
    }
}
