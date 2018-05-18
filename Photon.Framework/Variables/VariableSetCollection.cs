using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Variables
{
    [Serializable]
    public class VariableSetCollection
    {
        private readonly JsonSerializer serializer;

        public Dictionary<string, string> Json {get;}

        public VariableSet this[string name] => GetSet(name);


        public VariableSetCollection()
        {
            Json = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
        }

        public bool TryGetSet(string name, out VariableSet variableSet)
        {
            var result = Json.TryGetValue(name, out var json);
            variableSet = result ? VariableSet.Create(json, serializer) : null;
            return result;
        }

        public VariableSet GetSet(string name)
        {
            return Json.TryGetValue(name, out var json)
                ? VariableSet.Create(json, serializer) : null;
        }
    }
}
