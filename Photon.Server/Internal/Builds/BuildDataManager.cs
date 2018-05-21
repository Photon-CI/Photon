using log4net;
using System.Collections.Generic;
using System.IO;

namespace Photon.Server.Internal.Builds
{
    internal class BuildDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildDataManager));

        private readonly string buildPath;
        private readonly Dictionary<uint, BuildData> data;

        public IEnumerable<uint> AllNumbers => data.Keys;
        public IEnumerable<BuildData> AllBuilds => data.Values;


        public BuildDataManager(string buildPath)
        {
            this.buildPath = buildPath;

            data = new Dictionary<uint, BuildData>();
        }

        public void Load()
        {
            if (!Directory.Exists(buildPath)) return;

            foreach (var path in Directory.EnumerateDirectories(buildPath, "*", SearchOption.TopDirectoryOnly)) {
                var path_name = Path.GetFileName(path);

                if (!uint.TryParse(path_name, out var buildNumber)) {
                    Log.Warn($"Failed to parse build directory '{path_name}'!");
                    continue;
                }

                var buildData = new BuildData {
                    Number = buildNumber,
                    Path = path,
                };

                data[buildNumber] = buildData;
            }
        }

        public bool TryGet(uint number, out BuildData buildData)
        {
            return data.TryGetValue(number, out buildData);
        }
    }
}
