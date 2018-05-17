using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Photon.Framework.Tasks
{
    public abstract class TaskRegistryBase<T>
    {
        protected readonly Dictionary<string, Type> mapType;
        protected readonly Dictionary<string, TaskDescription> mapDesc;

        public IEnumerable<string> AllNames => mapType.Keys;
        public IEnumerable<TaskDescription> AllDescriptions => mapDesc.Values;


        protected TaskRegistryBase()
        {
            mapType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            mapDesc = new Dictionary<string, TaskDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetDescription(string taskName, out TaskDescription taskDescription)
        {
            return mapDesc.TryGetValue(taskName, out taskDescription);
        }

        public void ScanAssembly(Assembly assembly)
        {
            var scriptType = typeof(T);

            var scriptTypeList = assembly.DefinedTypes
                .Where(t => t.IsClass && scriptType.IsAssignableFrom(t))
                .ToArray();

            foreach (var type in scriptTypeList) {
                string[] roles = null;

                var roleAttr = type.GetCustomAttribute<RolesAttribute>();
                if (roleAttr != null) {
                    roles = roleAttr.Roles;
                }

                mapType[type.Name] = type;
                mapDesc[type.Name] = new TaskDescription {
                    Name = type.Name,
                    Roles = roles,
                };
            }
        }
    }
}
