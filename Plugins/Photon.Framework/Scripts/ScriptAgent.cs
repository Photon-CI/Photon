namespace Photon.Framework.Scripts
{
    public class ScriptAgent
    {
        public ScriptAgentSession BeginSession()
        {
            var session = new ScriptAgentSession();

            // TODO: Send Agent session/begin request

            return session;
        }
    }
}
