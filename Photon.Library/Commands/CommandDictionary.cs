using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Library.Commands
{
    public delegate Task CommandActionEventHandler(string[] args);
    public delegate Task CommandPropertyEventHandler(string value);

    public class CommandDictionary<TContext> : ICommandParser
    {
        protected readonly Dictionary<string, CommandActionEventHandler> ActionList;
        protected readonly Dictionary<string, CommandPropertyEventHandler> PropertyList;

        public TContext Context {get;}
        public bool ActionRequired {get; set;}


        public CommandDictionary(TContext context)
        {
            this.Context = context;

            ActionRequired = true;
            ActionList = new Dictionary<string, CommandActionEventHandler>(StringComparer.OrdinalIgnoreCase);
            PropertyList = new Dictionary<string, CommandPropertyEventHandler>(StringComparer.OrdinalIgnoreCase);
        }

        public CommandActionBuilder Map(params string[] names)
        {
            return new CommandActionBuilder(ActionList, PropertyList, names);
        }

        public async Task ParseAsync(params string[] args)
        {
            var argList = new List<string>(args);

            CommandActionEventHandler action = null;
            if (argList.Count > 0 && ActionList.TryGetValue(argList[0], out action))
                argList.RemoveAt(0);

            for (var i = argList.Count - 1; i >= 0; i--) {
                var arg = argList[i];

                string value = null;
                var x = arg.IndexOf('=');

                if (x >= 0) {
                    value = arg.Substring(x + 1).Trim('\"');
                    arg = arg.Substring(0, x);
                }

                if (!PropertyList.TryGetValue(arg, out var propertyAction)) continue;
                await propertyAction(value);
                argList.RemoveAt(i);
            }

            if (action != null) {
                await action(argList.ToArray());
            }
            else if (ActionRequired) {
                throw new ApplicationException("No action specified!");
            }
        }
    }
}
