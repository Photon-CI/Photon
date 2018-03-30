namespace Photon.Agent.Internal
{
    internal class Arguments
    {
        public bool RunAsConsole {get; private set;}


        public Arguments()
        {
            RunAsConsole = false;
        }

        public void Parse(string[] args)
        {
            RunAsConsole = true;
        }
    }
}
