using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Framework.Tools
{
    public static class PathEx
    {
        /// <summary>
        /// Creates the directory portion of the filename if it does not exist.
        /// </summary>
        public static void CreateFilePath(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var path = Path.GetDirectoryName(filename);

            if (path != null && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Creates a directory if it does not exist.
        /// </summary>
        public static void CreatePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        public static void Delete(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            if (File.Exists(filename))
                File.Delete(filename);
        }

        /// <summary>
        /// Deletes a directory if it exists, and all of its contents recursively.
        /// </summary>
        public static void DestoryDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            var errorList = new Lazy<List<Exception>>();

            try {
                DestoryDirectoryContents(path);
            }
            catch (Exception error) {
                errorList.Value.Add(error);
            }

            try {
                Directory.Delete(path, true);
            }
            catch (Exception error) {
                errorList.Value.Add(error);
            }

            if (errorList.IsValueCreated)
                throw new AggregateException(errorList.Value);
        }

        private static void DestoryDirectoryContents(string path)
        {
            var errorList = new Lazy<List<Exception>>();

            foreach (var subdir in Directory.GetDirectories(path)) {
                try {
                    DestoryDirectoryContents(subdir);
                }
                catch (Exception error) {
                    errorList.Value.Add(error);
                }

                try {
                    Directory.Delete(subdir);
                }
                catch (Exception error) {
                    errorList.Value.Add(error);
                }
            }

            foreach (var file in Directory.GetFiles(path)) {
                try {
                    var attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        File.SetAttributes(file, attr ^ FileAttributes.ReadOnly);

                    File.Delete(file);
                }
                catch (Exception error) {
                    errorList.Value.Add(error);
                }
            }

            if (errorList.IsValueCreated)
                throw new AggregateException(errorList.Value);
        }
    }
}
