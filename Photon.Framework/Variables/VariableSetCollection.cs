using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Framework.Variables
{
    [Serializable]
    public class VariableSetCollection
    {
        public Dictionary<string, string> JsonList {get;}
        public string GlobalJson {get; set;}

        public VariableSet Global => CreateSet(GlobalJson);


        public VariableSetCollection()
        {
            JsonList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string name, out VariableSet variableSet)
        {
            var result = JsonList.TryGetValue(name, out var json);
            variableSet = result ? CreateSet(json) : null;
            return result;
        }

        public VariableSet Get(string name)
        {
            return JsonList.TryGetValue(name, out var json)
                ? CreateSet(json) : null;
        }

        private VariableSet CreateSet(string json)
        {
            var serializer = new JsonSerializer();

            object variable;
            using (var reader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(reader)) {
                variable = serializer.Deserialize(jsonReader);
            }

            return new VariableSet(variable);
        }

        public VariableSet this[string name] => Get(name);
    }
}
