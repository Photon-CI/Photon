using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Extensions
{
    public static class ExceptionExtensions
    {
        public static string UnfoldMessages(this Exception error)
        {
            return string.Join(" ", UnfoldExceptions(error).Select(e => e.Message));
        }

        public static IEnumerable<Exception> UnfoldExceptions(this Exception error)
        {
            var e = error;
            while (e != null) {
                yield return e;
                e = e.InnerException;
            }
        }
    }
}
