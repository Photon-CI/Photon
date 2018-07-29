using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Framework.Tools.Content
{
    public class ContentFilter
    {
        public IContentProvider Provider {get; set;}
        public string SourceDirectory {get; set;}
        public string DestinationDirectory {get; set;}
        public List<string> IgnoredDirectories {get; set;}

        public Action<string, string> DirectoryAction {get; set;}
        public Action<string, string> FileAction {get; set;}


        public ContentFilter()
        {
            Provider = new SystemContentProvider();
            IgnoredDirectories = new List<string>();
        }

        public void Run()
        {
            if (string.IsNullOrEmpty(SourceDirectory))
                throw new ArgumentNullException(nameof(SourceDirectory));

            if (string.IsNullOrEmpty(DestinationDirectory))
                throw new ArgumentNullException(nameof(DestinationDirectory));

            if (!Provider.DirectoryExists(SourceDirectory))
                throw new ApplicationException($"Source directory '{SourceDirectory}' does not exist!");

            var ignoredPaths = IgnoredDirectories
                .Select(x => x.Split(Provider.DirectorySeparatorChar))
                .ToArray();

            CopyInner(SourceDirectory, DestinationDirectory, ignoredPaths);
        }

        private void CopyInner(string sourcePath, string destPath, string[][] ignoredPaths)
        {
            if (!Provider.DirectoryExists(destPath)) {
                DirectoryAction?.Invoke(sourcePath, destPath);
            }

            var ignoredSubPaths = ignoredPaths
                .Where(x => x.Length > 1)
                .Select(x => x.Skip(1).ToArray())
                .ToArray();

            foreach (var path in Provider.GetDirectories(sourcePath)) {
                var path_name = Path.GetFileName(path) ?? string.Empty;

                var isIgnored = ignoredPaths.Select(x => x[0])
                    .Any(x => string.Equals(path_name, x, StringComparison.OrdinalIgnoreCase));

                if (isIgnored) continue;

                var destSubPath = Path.Combine(destPath, path_name);
                CopyInner(path, destSubPath, ignoredSubPaths.ToArray());
            }
            
            var errorList = new List<Exception>();

            foreach (var file in Provider.GetFiles(sourcePath)) {
                var file_name = Path.GetFileName(file);
                var destFile = Path.Combine(destPath, file_name);

                try {
                    FileAction?.Invoke(file, destFile);
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }

            if (errorList.Any()) throw new AggregateException(errorList);
        }
    }
}
