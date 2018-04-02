using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.CLI.Internal.Commands
{
    public delegate Task CommandActionEventHandler(string[] args);
    public delegate Task CommandPropertyEventHandler(string value);

    internal class CommandDictionary<TContext> : ICommandParser
    {
        protected readonly Dictionary<string, CommandActionEventHandler> actionList;
        protected readonly Dictionary<string, CommandPropertyEventHandler> propertyList;

        public TContext Context {get;}


        public CommandDictionary(TContext context)
        {
            this.Context = context;

            actionList = new Dictionary<string, CommandActionEventHandler>(StringComparer.OrdinalIgnoreCase);
            propertyList = new Dictionary<string, CommandPropertyEventHandler>(StringComparer.OrdinalIgnoreCase);
        }

        public CommandActionBuilder Map(params string[] names)
        {
            return new CommandActionBuilder(actionList, propertyList, names);
        }

        public async Task ParseAsync(params string[] args)
        {
            var _args = new List<string>(args);

            CommandActionEventHandler action = null;
            if (_args.Count > 0 && actionList.TryGetValue(_args[0], out action))
                _args.RemoveAt(0);

            for (var i = _args.Count - 1; i >= 0; i--) {
                var arg = _args[i];

                string value = null;
                var x = arg.IndexOf('=');

                if (x >= 0) {
                    value = arg.Substring(x + 1).Trim('\"');
                    arg = arg.Substring(0, x);
                }

                if (!propertyList.TryGetValue(arg, out var propertyAction)) continue;
                await propertyAction(value);
                _args.RemoveAt(i);
            }

            if (action != null) {
                await action(_args.ToArray());
            }
            else {
                throw new ApplicationException("No action specified!");
            }
        }
    }
}
