using System;

namespace Photon.CLI.Internal.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommandAttribute : Attribute
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
    }
}
