using System;
using System.Text;

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
    }
}
