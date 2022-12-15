using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.Reflection;
using System.Resources;

namespace PlanSuite.Services
{
    public class LocalisationService
    {
        public static LocalisationService Instance { get; private set; }

        public Dictionary<Language, ResourceManager> LanguageResources { get; private set; } = new Dictionary<Language, ResourceManager>();

        public readonly Dictionary<Language, string> SupportedLanguages = new Dictionary<Language, string>
        {
            { Language.English, "En-Gb" }
        };

        public LocalisationService()
        {
            Instance = this;
            foreach (var language in SupportedLanguages)
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resName = $"PlanSuite.Resources.{language.Value}";
                LanguageResources.Add(language.Key, new ResourceManager(resName, assembly));
                Console.WriteLine($"Loaded language resource: {assembly.Location}/{resName}");
            }
        }


        public string Get(ApplicationUser? user, string key)
        {
            var userLanguage = Language.English;

            return LanguageResources[userLanguage].GetString(key);
        }
    }
}
