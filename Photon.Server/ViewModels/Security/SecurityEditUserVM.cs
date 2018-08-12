using Photon.Framework.Extensions;
using Photon.Library.Security;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels.Security
{
    internal class SecurityEditUserVM : ServerViewModel
    {
        public string UserId {get; set;}
        public string UserDisplayName {get; set;}
        public string UserUsername {get; set;}
        public string UserPassword {get; set;}
        public bool UserEnabled {get; set;}
        public bool UserDomainEnabled {get; set;}


        public SecurityEditUserVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Edit User Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            if (!string.IsNullOrEmpty(UserId)) {
                if (!PhotonServer.Instance.UserMgr.TryGetUserById(UserId, out var _user))
                    throw new ApplicationException($"User '{UserId}' not found!");

                UserDisplayName = _user.DisplayName;
                UserUsername = _user.Username;
                UserPassword = _user.Password;
                UserEnabled = _user.IsEnabled;
                UserDomainEnabled = _user.IsDomainEnabled;
            }
        }

        public void Restore(NameValueCollection form)
        {
            UserId = form.Get(nameof(UserId));
            UserDisplayName = form.Get(nameof(UserDisplayName));
            UserUsername = form.Get(nameof(UserUsername));
            UserPassword = form.Get(nameof(UserPassword));
            UserEnabled = form.Get(nameof(UserEnabled)).To<bool>();
            UserDomainEnabled = form.Get(nameof(UserDomainEnabled)).To<bool>();
        }

        public void Save()
        {
            var userMgr = PhotonServer.Instance.UserMgr;

            User user = null;
            if (!string.IsNullOrEmpty(UserId)) {
                userMgr.TryGetUserById(UserId, out user);
            }
            else {
                UserId = Guid.NewGuid().ToString("D");
            }

            if (user == null) {
                user = new User {
                    Id = UserId,
                };

                userMgr.AddUser(user);
            }

            user.DisplayName = UserDisplayName;
            user.Username = UserUsername;
            user.Password = UserPassword;
            user.IsEnabled = UserEnabled;
            user.IsDomainEnabled = UserDomainEnabled;

            userMgr.Save();
        }

        public void Delete()
        {
            var userMgr = PhotonServer.Instance.UserMgr;

            if (!userMgr.RemoveUser(UserId))
                throw new ApplicationException($"User '{UserId}' not found!");

            userMgr.Save();
        }
    }
}
