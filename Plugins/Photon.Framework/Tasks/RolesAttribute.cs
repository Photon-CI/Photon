using System;

namespace Photon.Framework.Tasks
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RolesAttribute : Attribute
    {
        public string[] Roles {get;}


        public RolesAttribute(params string[] roles)
        {
            this.Roles = roles;
        }
    }
}
