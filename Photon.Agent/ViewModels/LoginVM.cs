using Photon.Agent.Internal;
using Photon.Framework.Extensions;
using System.Collections.Specialized;

namespace Photon.Agent.ViewModels
{
    internal class LoginVM : AgentViewModel
    {
        public string Username {get; set;}
        public string Password {get; set;}
        public bool RememberMe {get; set;}
        public string AuthMessage {get; set;}


        public LoginVM()
        {
            PageTitle = "Photon Agent Login";
        }

        public void Restore(NameValueCollection formData)
        {
            Username = formData.Get(nameof(Username));
            Password = formData.Get(nameof(Password));
            RememberMe = formData.Get(nameof(RememberMe)).To<bool>();
        }
    }
}
