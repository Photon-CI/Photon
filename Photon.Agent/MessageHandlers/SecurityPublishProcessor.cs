using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Security;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class SecurityPublishProcessor : MessageProcessorBase<SecurityPublishRequest>
    {
        public override async Task<IResponseMessage> Process(SecurityPublishRequest requestMessage)
        {
            var config = PhotonAgent.Instance.AgentConfiguration;

            config.Value.Security.Enabled = requestMessage.SecurityEnabled;
            config.Value.Security.DomainEnabled = requestMessage.SecurityDomainEnabled;
            config.Save();

            PhotonAgent.Instance.Http.Security.Restricted = config.Value.Security.Enabled;
            PhotonAgent.Instance.Http.Authorization.DomainEnabled = config.Value.Security.DomainEnabled;
            // TODO: Update security of any additional related components

            try {
                var userMgr = PhotonAgent.Instance.UserMgr;
                userMgr.SetData(requestMessage.UserGroups, requestMessage.Users);
                userMgr.Save();
            }
            catch (Exception error) {
                return new ResponseMessageBase {
                    ExceptionMessage = "Failed to apply user security configuration!",
                    Exception = error.ToString(),
                    Successful = false,
                };
            }

            return new ResponseMessageBase {
                Successful = true,
            };
        }
    }
}
