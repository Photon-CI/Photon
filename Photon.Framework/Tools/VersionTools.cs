using System;

namespace Photon.Framework.Tools
{
    public static class VersionTools
    {
        public static bool HasUpdates(string currentVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(currentVersion)) return true;

            if (string.IsNullOrEmpty(newVersion))
                throw new ArgumentNullException(nameof(newVersion));

            var _current = new Version(currentVersion);
            var _new = new Version(newVersion);

            return _new > _current;
        }
    }
}
