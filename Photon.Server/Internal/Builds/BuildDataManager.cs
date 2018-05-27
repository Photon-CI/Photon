using System;
using log4net;
using System.Collections.Generic;
using System.IO;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Framework.Extensions;

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

                var indexFilename = Path.Combine(path, "index.json");
                BuildData buildData;

                if (File.Exists(indexFilename)) {
                    using (var stream = File.Open(indexFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        buildData = JsonSettings.Serializer.Deserialize<BuildData>(stream);

                        //buildData.Number = buildNumber;
                        buildData.ContentPath = path;
                    }
                }
                else {
                    buildData = new BuildData {
                        Number = buildNumber,
                        ContentPath = path,
                    };
                }

                data[buildNumber] = buildData;
            }
        }

        public BuildData New(uint number)
        {
            var _path = Path.Combine(buildPath, number.ToString());
            PathEx.CreatePath(_path);

            var buildData = new BuildData {
                Number = number,
                ContentPath = _path,
                Created = DateTime.UtcNow,
            };

            data[number] = buildData;
            return buildData;
        }

        public bool TryGet(uint number, out BuildData buildData)
        {
            return data.TryGetValue(number, out buildData);
        }
    }
}
