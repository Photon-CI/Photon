using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Photon.Framework.Domain
{
    internal class TypeRegistry<T>
    {
        protected readonly Dictionary<string, Type> map;


        public TypeRegistry()
        {
            map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public void ScanAssembly(Assembly assembly)
        {
            var scriptType = typeof(T);

            var scriptTypeList = assembly.DefinedTypes
                .Where(t => t.IsClass && scriptType.IsAssignableFrom(t))
                .ToArray();

            foreach (var type in scriptTypeList)
                map[type.Name] = type;
        }
    }
}
