namespace Photon.Server.Internal
{
    internal static class Arguments
    {
        public static bool RunAsConsole {get; private set;}


        static Arguments()
        {
            RunAsConsole = false;
        }

        public static void Parse(string[] args)
        {
            RunAsConsole = true;
        }
    }
}
