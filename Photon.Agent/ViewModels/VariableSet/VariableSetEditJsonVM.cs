using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels.VariableSet
{
    internal class VariableSetEditJsonVM : AgentViewModel
    {
        public string SetId {get; set;}


        public VariableSetEditJsonVM(IHttpHandler handler) : base(handler) {}
    }
}
