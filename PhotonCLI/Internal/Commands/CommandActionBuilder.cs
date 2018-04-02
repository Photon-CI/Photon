using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.CLI.Internal.Commands
{
    internal class CommandActionBuilder
    {
        private readonly IDictionary<string, CommandActionEventHandler> actionList;
        private readonly IDictionary<string, CommandPropertyEventHandler> propertyList;
        private readonly string[] names;


        public CommandActionBuilder(IDictionary<string, CommandActionEventHandler> actionList, IDictionary<string, CommandPropertyEventHandler> propertyList, string[] names)
        {
            this.actionList = actionList;
            this.propertyList = propertyList;
            this.names = names;
        }

        public void ToProperty(Action<string> action)
        {
            foreach (var name in names) {
                propertyList[name] = async v => {
                    await Task.Run(() => action(v));
                };
            }
        }

        public void ToProperty<T>(Action<T> action, T defaultValue = default(T))
        {
            foreach (var name in names) {
                propertyList[name] = async v => {
                    var value = v != null
                        ? v.To<T>()
                        : defaultValue;

                    await Task.Run(() => action(value));
                };
            }
        }

        public void ToProperty(CommandPropertyEventHandler action)
        {
            foreach (var name in names)
                propertyList[name] = action;
        }

        public void ToAction(ICommandParser parser)
        {
            foreach (var name in names)
                actionList[name] = parser.ParseAsync;
        }

        public void ToAction(CommandActionEventHandler action)
        {
            foreach (var name in names)
                actionList[name] = action;
        }
    }
}
