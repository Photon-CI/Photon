using Photon.Server.Internal;

namespace Photon.Server.ViewModels.VariableSet
{
    internal class VariablesEditJsonVM : ServerViewModel
    {
        public string SetId {get; set;}

        public bool IsNew => string.IsNullOrEmpty(SetId);
    }
}
