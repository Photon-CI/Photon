namespace Photon.Framework.Applications
{
    public class DomainApplicationManager
    {
        private readonly IApplicationManager appMgr;


        public DomainApplicationManager(IApplicationManager appMgr)
        {
            this.appMgr = appMgr;
        }

        public bool TryGetApplication(string projectId, string appName, out DomainApplication application)
        {
            var app = appMgr.GetApplication(projectId, appName);
            application = app == null ? null : new DomainApplication(app);
            return app != null;
        }

        public DomainApplication RegisterApplication(string projectId, string appName)
        {
            var app = appMgr.RegisterApplication(projectId, appName);

            return new DomainApplication(app);
        }
    }
}
