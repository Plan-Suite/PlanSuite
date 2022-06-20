using System.IO;

namespace PlanSuite.Utility
{
    public class ConfigReader
    {
        private string m_ConfigFile;
        public Dictionary<string, string> Entries { get; private set; } = new Dictionary<string, string>();

        public ConfigReader(string path)
        {
            m_ConfigFile = path;

            if (!File.Exists(m_ConfigFile))
            {
                return;
            }

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith('#'))
                {
                    // Ignore comments
                    continue;
                }
                string[] texts = lines[i].Split('=');
                if (texts.Length != 2)
                {
                    // Ignore invalid line
                    continue;
                }

                string key = texts[0];
                string value = texts[1];
                if (Entries.ContainsKey(key))
                {
                    Entries[key] = value;
                }
                else
                {
                    Entries.Add(key, value);
                }
            }
        }

        public int GetInt(string key)
        {
            if (Entries.ContainsKey(key))
            {
                if (int.TryParse(Entries[key], out int result))
                {
                    return result;
                }
            }
            return 0;
        }


        public string GetSecret(string key)
        {
            if (Entries.ContainsKey(key))
            {
                return Entries[key];
            }
            return string.Empty;
        }
    }
}
