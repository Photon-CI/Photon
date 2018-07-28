using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Framework.Tools
{
    public class DirectoryCopy
    {
        public string SourceDirectory {get; set;}
        public string DestinationDirectory {get; set;}
        public List<string> IgnoredDirectories {get; set;}


        public DirectoryCopy()
        {
            IgnoredDirectories = new List<string>();
        }

        public void Copy()
        {
            if (string.IsNullOrEmpty(SourceDirectory))
                throw new ArgumentNullException(nameof(SourceDirectory));

            if (string.IsNullOrEmpty(DestinationDirectory))
                throw new ArgumentNullException(nameof(DestinationDirectory));

            var errorList = new List<Exception>();

            foreach (var path in Directory.GetDirectories(SourceDirectory, "*", SearchOption.AllDirectories)) {
                var isIgnored = false;
                foreach (var ignorePath in IgnoredDirectories) {
                    var ignorePathSource = Path.Combine(SourceDirectory, ignorePath);
                    if (string.Equals(ignorePathSource, path, StringComparison.OrdinalIgnoreCase)) {
                        isIgnored = true;
                        break;
                    }
                }

                if (isIgnored) continue;

                var newPath = path.Replace(SourceDirectory, DestinationDirectory);
                Directory.CreateDirectory(newPath);
            }

            foreach (var file in Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories)) {
                var sourcePath = Path.GetDirectoryName(file);

                var isIgnored = false;
                foreach (var ignorePath in IgnoredDirectories) {
                    var ignorePathSource = Path.Combine(SourceDirectory, ignorePath);
                    if (string.Equals(ignorePathSource, sourcePath, StringComparison.OrdinalIgnoreCase)) {
                        isIgnored = true;
                        break;
                    }
                }

                if (isIgnored) continue;

                var newPath = file.Replace(SourceDirectory, DestinationDirectory);

                try {
                    File.Copy(file, newPath, true);
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }

            if (errorList.Any()) throw new AggregateException(errorList);
        }
    }
}
