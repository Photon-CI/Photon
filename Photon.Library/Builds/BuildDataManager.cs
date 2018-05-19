using System.Collections.Generic;

namespace Photon.Library.Builds
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
