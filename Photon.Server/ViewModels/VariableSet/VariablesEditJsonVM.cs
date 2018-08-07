using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariablesEditJsonVM : ServerViewModel
    {
        public string SetId {get; set;}

        public bool IsNew => string.IsNullOrEmpty(SetId);


        public VariablesEditJsonVM(IHttpHandler handler) : base(handler) {}
    }
}
