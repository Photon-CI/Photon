using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;

namespace Photon.Worker.Internal
{
    internal class ProcessArguments
    {
        private readonly Dictionary<string, string> map;

        public string PipeInHandle => GetValue("-in-handle");
        public string PipeOutHandle => GetValue("-out-handle");


        public ProcessArguments()
        {
            map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        
        public void Process(string[] args)
        {
            MapArguments(args);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            return map.TryGetValue(key, out var value)
                ? value : defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            return map.TryGetValue(key, out var value)
                ? value.To<T>() : defaultValue;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(PipeInHandle))
                throw new ApplicationException("-in-handle is undefined!");

            if (string.IsNullOrEmpty(PipeOutHandle))
                throw new ApplicationException("-out-handle is undefined!");
        }

        private void MapArguments(IEnumerable<string> args)
        {
            foreach (var arg in args) {
                var i = arg.IndexOf('=');

                var _key = i < 0 ? arg : arg.Substring(0, i);
                var _val = i < 0 ? null : arg.Substring(i + 1);

                if (_val != null && _val.Length >= 2) {
                    if (_val[0] == '\'' && _val[_val.Length - 1] == '\'')
                        _val = _val.Substring(1, _val.Length - 2);
                    else if (_val[0] == '\"' && _val[_val.Length - 1] == '\"')
                        _val = _val.Substring(1, _val.Length - 2);
                }

                map[_key] = _val;
            }
        }
    }
}
