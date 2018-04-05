using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Photon.Framework
{
    public static class NetPath
    {
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                throw new ArgumentNullException(nameof(paths));

            var builder = new StringBuilder();

            for (var i = 0; i < paths.Length; i++) {
                var p = paths[i];

                var c = builder.Length;

                if (c == 0) {
                    builder.Append(p);
                    continue;
                }

                var endsWithSep = c > 0 && builder[c - 1] == '/';
                var startsWithSep = p.StartsWith("/");

                if (endsWithSep) {
                    if (startsWithSep) p = p.Substring(1);
                }
                else {
                    if (!startsWithSep) builder.Append("/");
                }

                builder.Append(p);
            }

            return builder.ToString();
        }

        public static string QueryString(object arguments)
        {
            var args = (arguments as IDictionary<string, object>)
                ?? arguments.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(arguments)))
                    .ToDictionary(x => x.Key, x => x.Value);

            if (!args.Any()) return string.Empty;

            var builder = new StringBuilder("?");

            var i = 0;
            foreach (var arg in args) {
                if (arg.Value == null) continue;
                if (i > 0) builder.Append("&");
                i++;

                builder.Append(HttpUtility.UrlEncode(arg.Key));
                builder.Append("=");
                builder.Append(HttpUtility.UrlEncode(arg.Value.ToString()));
            }

            return builder.ToString();
        }
    }
}
