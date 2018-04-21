using System;
using System.Collections.Generic;
using System.IO;

namespace Photon.Library
{
    public static class FileUtils
    {
        public static void DestoryDirectory(string path)
        {
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
