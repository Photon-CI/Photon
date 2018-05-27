using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Framework.Variables
{
    [Serializable]
    public class VariableSetCollection
    {
        public Dictionary<string, string> Json {get;}

        public VariableSet this[string name] => GetSet(name);


        public VariableSetCollection()
        {
            Json = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetSet(string name, out VariableSet variableSet)
        {
            var result = Json.TryGetValue(name, out var json);
            variableSet = result ? CreateSet(json) : null;
            return result;
        }

        public VariableSet GetSet(string name)
        {
            return Json.TryGetValue(name, out var json)
                ? CreateSet(json) : null;
        }

        private static VariableSet CreateSet(string json)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

            using (var reader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(reader)) {
                var variable = serializer.Deserialize(jsonReader);
                return new VariableSet(variable);
            }
        }
    }
}
