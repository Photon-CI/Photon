﻿using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.GitHub;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Builds
{
    internal class BuildData
    {
        private const string outputFilename = "output.txt";

        private LazyAsync<string> outputData;

        [JsonIgnore]
        public string ContentPath {get; set;}

        public uint Number {get; set;}
        public DateTime Created {get; set;}
        public TimeSpan? Duration {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public string[] TaskRoles {get; set;}
        public string PreBuildCommand {get; set;}
        public string AssemblyFilename {get; set;}
        public string ServerSessionId {get; set;}
        public PackageReference[] ProjectPackages {get; set;}
        public PackageReference[] ApplicationPackages {get; set;}
        public GithubCommit Commit {get; set;}
        public bool IsComplete {get; set;}
        public bool IsSuccess {get; set;}
        public bool IsCancelled {get; set;}
        public string Exception {get; set;}


        public BuildData()
        {
            outputData = new LazyAsync<string>(LoadOutput);
        }

        public void Save()
        {
            var indexFilename = Path.Combine(ContentPath, "index.json");

            using (var stream = File.Open(indexFilename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, this);
            }
        }

        public async Task<string> GetOutput()
        {
            return await outputData;
        }

        public async Task SetOutput(string text)
        {
            outputData = new LazyAsync<string>(() => Task.FromResult(text));

            var outputFile = Path.Combine(ContentPath, outputFilename);
            PathEx.CreateFilePath(outputFile);
            
            using (var stream = File.Open(outputFile, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) {
                await writer.WriteAsync(text);
            }
        }

        private async Task<string> LoadOutput()
        {
            var outputFile = Path.Combine(ContentPath, outputFilename);
            
            using (var stream = File.Open(outputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
