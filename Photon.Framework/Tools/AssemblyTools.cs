using System;
using System.Reflection;

namespace Photon.Framework.Tools
{
    public static class AssemblyTools
    {
        public static Version GetVersion(string assemblyInfoFilename)
        {
            return AssemblyName.GetAssemblyName(assemblyInfoFilename).Version;
        }
    }
}
