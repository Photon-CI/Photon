using Photon.Library.Extensions;
using System.Configuration;

namespace Photon.Library
{
    public static class ConfigurationReader
    {
        public static string AppSetting(string key, string defaultValue = null)
        {
            var value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value;
        }

        public static T AppSetting<T>(string key, T defaultValue = default(T))
        {
            var value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value.To<T>();
        }
    }
}
