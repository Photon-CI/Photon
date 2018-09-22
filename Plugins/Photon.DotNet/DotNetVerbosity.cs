namespace Photon.DotNetPlugin
{
    public class DotNetVerbosity
    {
        public static string GetString(DotNetVerbosityLevel level)
        {
            switch (level) {
                case DotNetVerbosityLevel.Quiet: return "q";
                case DotNetVerbosityLevel.Minimum: return "m";
                case DotNetVerbosityLevel.Detailed: return "d";
                case DotNetVerbosityLevel.Diagnostic: return "diag";
                case DotNetVerbosityLevel.Normal:
                default: return "n";
            }
        }
    }

    public enum DotNetVerbosityLevel
    {
        Quiet,
        Minimum,
        Normal,
        Detailed,
        Diagnostic,
    }
}
