using System;

namespace Photon.Library.Extensions
{
    public static class StringExtensions
    {
        public static T To<T>(this string text, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(text)) return defaultValue;

            var type = typeof(T);
            var nullableType = Nullable.GetUnderlyingType(type);

            if (nullableType != null) {
                type = nullableType;
            }

            if (type.IsEnum)
                return (T)Enum.Parse(type, text, true);

            if (type == typeof(bool)) {
                if (string.Equals(text, "on")) return (T)(object)true;
                if (string.Equals(text, "off")) return (T)(object)false;
            }

            return (T)Convert.ChangeType(text, type);
        }
    }
}
