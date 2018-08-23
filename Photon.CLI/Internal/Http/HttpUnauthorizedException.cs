using System;

namespace Photon.CLI.Internal.Http
{
    internal class HttpUnauthorizedException : ApplicationException
    {
        public HttpUnauthorizedException() : base("Request Unauthorized!") {}
    }
}
