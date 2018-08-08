using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Project
{
    internal class ProjectJsonVM : ServerViewModel
    {
        public string ProjectId {get; set;}


        public ProjectJsonVM(IHttpHandler handler) : base(handler) {}
    }
}
