using System;

namespace Photon.Framework.Tools
{
    public static class VersionTools
    {
        public static bool HasUpdates(string currentVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(currentVersion)) return true;

            var _current = new Version(currentVersion);
            var _new = new Version(newVersion);

            return _new > _current;
        }
    }
}
