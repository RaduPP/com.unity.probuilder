using System.IO;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderSettings
    {
        internal const string k_LegacySettingsPath = "ProjectSettings/ProBuilderSettings.json";
        const string k_PackageName = "com.unity.probuilder";

        static Settings s_Instance;

        internal static Settings instance
        {
            get
            {
                if (s_Instance == null)
                {
                    CheckForSettingsInRoot();
                    s_Instance = new Settings(k_PackageName);
                }

                return s_Instance;
            }
        }

        public static void Save()
        {
            instance.Save();
        }

        public static void Set<T>(string key, T value, SettingsScope scope = SettingsScope.Project)
        {
            instance.Set<T>(key, value, scope);
        }

        public static T Get<T>(string key, SettingsScope scope = SettingsScope.Project, T fallback = default(T))
        {
            return instance.Get<T>(key, scope, fallback);
        }

        public static bool ContainsKey<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            return instance.ContainsKey<T>(key, scope);
        }

        public static void Delete<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            instance.DeleteKey<T>(key, scope);
        }

        static void CheckForSettingsInRoot()
        {
            var path = ProjectSettingsRepository.GetSettingsPath(k_PackageName);

            // Only copy old settings if there are not existing settings in the new place. This prevents the situation
            // where VCS restores an old setting file and overwrites the current (correct) settings.
            if (!File.Exists(path) && File.Exists(k_LegacySettingsPath))
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(dir);
                    File.Move(k_LegacySettingsPath, path);
                    File.Delete(k_LegacySettingsPath);
                }
                catch(System.Exception e)
                {
                    Log.Warning(string.Format("Failed moving ProBuilder project settings file. To fix this warning, " +
                        "either manually move \"{0}\" to \"{1}\", or delete the file.\n\n{2}",
                        k_LegacySettingsPath,
                        path,
                        e.ToString()));
                }
            }
        }
    }
}
