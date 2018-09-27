using Photon.Communication;
using Photon.Library.TcpMessages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Library.Communication
{
    public static class ClientHandshake
    {
        private const int HandshakeTimeoutSec = 30;


        public static async Task Verify(MessageClient client, string serverVersion, CancellationToken token = default)
        {
            var handshakeRequest = new HandshakeRequest {
                Key = Guid.NewGuid().ToString(),
                ServerVersion = serverVersion,
            };

            var handshakeTimeout = TimeSpan.FromSeconds(HandshakeTimeoutSec);
            var handshakeResponse = await client.Handshake<HandshakeResponse>(handshakeRequest, handshakeTimeout, token);

            if (!string.Equals(handshakeRequest.Key, handshakeResponse.Key, StringComparison.Ordinal))
                throw new ApplicationException("Handshake Failed! An invalid key was returned.");

            if (!handshakeResponse.PasswordMatch)
                throw new ApplicationException("Handshake Failed! Unauthorized.");
        }
    }
}
