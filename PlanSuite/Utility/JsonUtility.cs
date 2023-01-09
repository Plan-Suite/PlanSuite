using System.Text.Json;

namespace PlanSuite.Utility
{
    public static class JsonUtility
    {
        /// <summary>
        /// Helper class to convert an object to a json string
        /// </summary>
        /// <param name="obj">The object to convert to a json string</param>
        /// <param name="prettyPrint">If true, make the json more readable. Default is false</param>
        /// <returns><b>string</b> The objects data in JSON format</returns>
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = prettyPrint;
            return JsonSerializer.Serialize(obj, options);
        }

        public static T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
