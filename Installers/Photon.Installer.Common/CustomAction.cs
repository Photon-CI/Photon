using Microsoft.Deployment.WindowsInstaller;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Photon.Installer.Common
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult LoadServerUrl(Session session)
        {
            session.Log($"Begin {nameof(LoadServerUrl)}");

            try {
                var pathInstall = session["INSTALLDIR"];

                if (string.IsNullOrEmpty(pathInstall))
                    throw new ApplicationException("'INSTALLDIR' is undefined!");

                var filename = Path.Combine(pathInstall, "server.json");

                session.Message(InstallMessage.Error, new Record {FormatString = $"Opening file '{filename}'..."});

                dynamic config;
                try {
                    config = ParseJsonFile(filename);
                }
                catch (Exception error) {
                    throw new ApplicationException($"Failed to parse JSON file '{filename}'! {error.Message}", error);
                }

                var http_host = (string)config?.http?.host ?? "localhost";
                var http_port = (string)config?.http?.port ?? "8082";
                var http_path = (string)config?.http?.path ?? "photon/server";

                var url = $"http://{http_host}:{http_port}";

                if (!string.IsNullOrEmpty(http_path)) {
                    if (!http_path.StartsWith("/"))
                        url += "/";

                    url += http_path;
                }

                session["PHOTON_URL"] = url;

                session.Log($"End {nameof(LoadServerUrl)}");
                return ActionResult.Success;
            }
            catch (Exception error) {
                session.Log($"Catch {nameof(LoadServerUrl)}: {error.Message}");
                session.Message(InstallMessage.Error, new Record {FormatString = error.Message});
                throw;
            }
        }

        [CustomAction]
        public static ActionResult LoadAgentUrl(Session session)
        {
            session.Log($"Begin {nameof(LoadAgentUrl)}");

            try {
                var pathInstall = session["INSTALLDIR"];

                if (string.IsNullOrEmpty(pathInstall))
                    throw new ApplicationException("'INSTALLDIR' is undefined!");

                var filename = Path.Combine(pathInstall, "agent.json");

                session.Message(InstallMessage.Error, new Record {FormatString = $"Opening file '{filename}'..."});

                dynamic config;
                try {
                    config = ParseJsonFile(filename);
                }
                catch (Exception error) {
                    throw new ApplicationException($"Failed to parse JSON file '{filename}'! {error.Message}", error);
                }

                var http_host = (string)config?.http?.host ?? "localhost";
                var http_port = (string)config?.http?.port ?? "8082";
                var http_path = (string)config?.http?.path ?? "photon/agent";

                var url = $"http://{http_host}:{http_port}";

                if (!string.IsNullOrEmpty(http_path)) {
                    if (!http_path.StartsWith("/"))
                        url += "/";

                    url += http_path;
                }

                session["PHOTON_URL"] = url;

                session.Log($"End {nameof(LoadAgentUrl)}");
                return ActionResult.Success;
            }
            catch (Exception error) {
                session.Log($"Catch {nameof(LoadAgentUrl)}: {error.Message}");
                session.Message(InstallMessage.Error, new Record {FormatString = error.Message});
                throw;
            }
        }

        private static dynamic ParseJsonFile(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) 
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader)) {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(jsonReader);
            }
        }
    }
}
