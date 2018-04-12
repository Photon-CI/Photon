using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Photon.Framework
{
    public static class JsonSettings
    {
        private static readonly Lazy<JsonSerializer> _serializer;

        public static JsonSerializer Serializer => _serializer.Value;


        static JsonSettings()
        {
            _serializer = new Lazy<JsonSerializer>(() => new JsonSerializer {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
        }
    }
}
