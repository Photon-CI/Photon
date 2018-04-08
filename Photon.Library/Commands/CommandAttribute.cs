using System;
using System.Linq;

namespace Photon.Library.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name {get; set;}
        public string Description {get; set;}


        public CommandAttribute(string name)
        {
            this.Name = name;
        }

        public CommandAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public static string GetName(Type type)
        {
            return GetAttribute(type)?.Name;
        }

        public static string GetName(Type type, string methodName)
        {
            return GetAttribute(type, methodName)?.Name;
        }

        public static string GetDescription(Type type)
        {
            return GetAttribute(type)?.Description;
        }

        public static string GetDescription(Type type, string methodName)
        {
            return GetAttribute(type, methodName)?.Description;
        }

        private static CommandAttribute GetAttribute(Type type)
        {
            return type
                .GetCustomAttributes(typeof(CommandAttribute), true)
                .FirstOrDefault() as CommandAttribute;
        }

        private static CommandAttribute GetAttribute(Type type, string methodName)
        {
            return type.GetMethod(methodName)?
                .GetCustomAttributes(typeof(CommandAttribute), true)
                .FirstOrDefault() as CommandAttribute;
        }
    }
}
