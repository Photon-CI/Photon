﻿using System;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public abstract class DomainAgentBase : MarshalByRefObject
    {
        public DomainAgentBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Domain_OnAssemblyResolve;
        }

        public virtual Assembly LoadAssembly(string filename)
        {
            return Assembly.LoadFile(filename);
        }

        private Assembly Domain_OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            Assembly _assembly = null;

            try {
                _assembly = Assembly.Load(e.Name);
            }
            catch {}

            return _assembly;
        }
    }
}
