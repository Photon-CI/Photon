using System;

namespace Photon.Framework.Extensions
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

        public static string Truncate(this string text, int maxLength)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

            return text.Length > maxLength
                ? text.Substring(0, maxLength)
                : text;
        }
    }
}
