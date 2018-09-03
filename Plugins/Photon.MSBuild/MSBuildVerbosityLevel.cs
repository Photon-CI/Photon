namespace Photon.MSBuild
{
    public static class MSBuildVerbosityLevel
    {
        public static string GetString(MSBuildVerbosityLevels level)
        {
            switch (level) {
                case MSBuildVerbosityLevels.Quiet: return "q";
                case MSBuildVerbosityLevels.Minimal: return "m";
                case MSBuildVerbosityLevels.Detailed: return "d";
                case MSBuildVerbosityLevels.Diagnostic: return "diag";
                default:
                case MSBuildVerbosityLevels.Normal: return "n";
            }
        }
    }

    public enum MSBuildVerbosityLevels
    {
        Normal,
        Minimal,
        Quiet,
        Detailed,
        Diagnostic,
    }
}
