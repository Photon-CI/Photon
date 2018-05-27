using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using Photon.Server.Internal.Builds;
using System.IO;
using Photon.Framework.Tools;

namespace Photon.Server.Internal.Projects
{
    internal class ServerProject
    {
        public string ContentPath {get; set;}
        public Project Description {get; set;}
        public BuildData Builds {get; set;}

        public string DescriptionFilename => Path.Combine(ContentPath, "project.json");


        public void Load()
        {
            if (!File.Exists(DescriptionFilename)) {
                Description = null;
                return;
            }

            using (var stream = File.Open(DescriptionFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                Description = JsonSettings.Serializer.Deserialize<Project>(stream);
            }

            // TODO: Load build data
        }

        public void Save()
        {
            PathEx.CreatePath(ContentPath);

            using (var stream = File.Open(DescriptionFilename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, Description);
            }
        }
    }
}
