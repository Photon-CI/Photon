using System.Collections.Generic;

namespace Photon.Server.Internal.Builds
{
    internal class BuildDataManager
    {
        private readonly Dictionary<int, BuildData> data;


        public BuildDataManager()
        {
            data = new Dictionary<int, BuildData>();
        }
    }
}
