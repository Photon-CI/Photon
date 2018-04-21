using System.Diagnostics;

namespace Photon.Framework.Tools
{
    public static class AssemblyTools
    {
        public static string GetVersion(string assemblyInfoFilename)
        {
            return FileVersionInfo.GetVersionInfo(assemblyInfoFilename).ProductVersion;
        }
    }
}
