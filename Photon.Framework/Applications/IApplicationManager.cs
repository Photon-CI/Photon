namespace Photon.Framework.Applications
{
    public interface IApplicationManager
    {
        Application GetApplication(string projectId, string appName);
        Application RegisterApplication(string projectId, string appName);
    }
}
