using Modding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MylaFlower
{
    internal static class Localization
    {
        private static readonly ILogger _logger = new SimpleLogger("MylaFlower.Localization");
        private static Dictionary<string, Dictionary<string, string>> _localizationData;

        public static void LoadResources()
        {
            _localizationData = new();

            LoadEmbeddedResources();
            LoadLocalResources();
        }

        private static Dictionary<string, string> LoadFromStream(Stream s)
        {
            using StreamReader sr = new(s);
            using JsonTextReader jtr = new(sr);

            JsonSerializer js = new();

            Dictionary<string, string> data = js.Deserialize<Dictionary<string, string>>(jtr);

            return data;
        }

        private static void LoadLocalResources()
        {
            Assembly asm = typeof(Localization).Assembly;
            DirectoryInfo modDirectory = new(Path.GetDirectoryName(asm.Location));
            foreach (FileInfo f in modDirectory.EnumerateFiles("*.json"))
            {
                const string suffix = ".json";
                
                string lang = f.Name.Substring(0, f.Name.Length - suffix.Length).ToUpperInvariant();
                _logger.LogDebug($"Loading local language data for {lang}");
                using Stream stream = f.OpenRead();

                _localizationData[lang] = LoadFromStream(stream);
            }
        }

        private static void LoadEmbeddedResources()
        {
            Assembly asm = typeof(Localization).Assembly;

            foreach (string name in asm.GetManifestResourceNames())
            {
                const string prefix = "MylaFlower.Resources.lang.";
                const string suffix = ".json";
                if (!name.StartsWith(prefix)) continue;
                if (!name.EndsWith(suffix)) continue;

                string lang = name.Substring(prefix.Length, name.Length - prefix.Length - suffix.Length).ToUpperInvariant();
                _logger.LogDebug($"Loading embedded language data for {lang}");

                using Stream stream = asm.GetManifestResourceStream(name);

                _localizationData[lang] = LoadFromStream(stream);
            }
        }

        public static string GetText(string key)
        {
            string lang = Language.Language.CurrentLanguage().ToString().ToUpperInvariant();
            if (!_localizationData.ContainsKey(lang))
            {
                lang = "EN";
            }

            return _localizationData[lang][key];
        }
    }
}
