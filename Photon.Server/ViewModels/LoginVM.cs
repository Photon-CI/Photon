using Photon.Framework.Extensions;
using Photon.Server.Internal;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels
{
    internal class LoginVM : ServerViewModel
    {
        public string Username {get; set;}
        public string Password {get; set;}
        public bool RememberMe {get; set;}
        public string AuthMessage {get; set;}


        public LoginVM()
        {
            PageTitle = "Photon Server Login";
        }

        public void Restore(NameValueCollection formData)
        {
            Username = formData.Get(nameof(Username));
            Password = formData.Get(nameof(Password));
            RememberMe = formData.Get(nameof(RememberMe)).To<bool>();
        }
    }
}
