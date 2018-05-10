using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using PiServerLite.Http.Handlers;
using System.IO;

namespace Photon.Library.Extensions
{
    public static class HttpResultExtensions
    {
        public static HttpHandlerResult Json(this ResponseBuilder builder, object data)
        {
            return Json(builder, data, JsonSettings.Serializer);
        }

        public static HttpHandlerResult Json(this ResponseBuilder builder, object data, JsonSerializer serializer)
        {
            return builder.Ok()
                .SetContentType("application/json")
                .SetContent(async (response, token) => {
                    using (var buffer = new MemoryStream()) {
                        serializer.Serialize(buffer, data, true);
                        buffer.Seek(0, SeekOrigin.Begin);

                        response.SetLength(buffer.Length);
                        await buffer.CopyToAsync(response.GetStream());
                    }
                });
        }
    }
}
