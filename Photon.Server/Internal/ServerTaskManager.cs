using Newtonsoft.Json;
using Photon.Library;
using Photon.Library.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerTaskManager : IDisposable
    {
        private readonly TaskPool<ServerTask> taskPool;

        public IEnumerable<ServerTask> Tasks => taskPool.Tasks;


        public ServerTaskManager()
        {
            taskPool = new TaskPool<ServerTask>();
        }

        public void Initialize()
        {
            taskPool.Start();
        }

        public void Dispose()
        {
            taskPool?.Dispose();
        }

        public async Task<ServerStartResponse> StartTask(string projectName, string taskName)
        {
            //return await TaskRunner.StartTask(projectName, taskName);

            // Find Project
            var project = Program.Server?.FindProject(projectName);

            if (project == null) throw new ApplicationException($"Project '{projectName}' not found!");

            // Find Task
            var task = project.FindTask(taskName);

            if (task == null) throw new ApplicationException($"Task '{taskName}' not found in Project '{projectName}'!");

            // Find Agents
            var agentList = Program.Server?.Definition?.Agents?.ToArray();

            if (agentList == null || !agentList.Any())
                throw new ApplicationException("No Agents are currently configured on this Server instance!");

            // Find Agents in Project Roles
            var projectsAgents = agentList.Where(agent => agent.MatchesRoles(task.Roles)).ToArray();

            if (!projectsAgents.Any()) {
                var roleNames = string.Join("; ", task.Roles);
                throw new ApplicationException($"No Agents were found matching roles [{roleNames}]!");
            }

            // Build Request
            var agentRequest = new AgentStartRequest {
                Project = {
                    Name = project.Name,
                },
                Task = {
                    Name = task.Name,
                    Type = task.Type,
                    File = task.File,
                    Roles = task.Roles,
                },
            };

            //--------------------

            var taskList = new List<Task>();
            var responseList = new ConcurrentBag<AgentStartAgentResponse>();
            var errorList = new ConcurrentBag<Exception>();

            var response = new AgentStartResponse {
                TaskId = "?",
            };

            foreach (var agent in projectsAgents) {
                taskList.Add(Task.Run(async () => {
                    try {
                        response.Agents.Add(new AgentStartAgentResponse {
                            TaskId = "?",
                            Name = agent.Name,
                        });

                        var agentResponse = await SendAsync<AgentStartRequest, AgentStartAgentResponse>(agent.Address, agentRequest);

                        responseList.Add(agentResponse);
                    }
                    catch (Exception error) {
                        errorList.Add(error);
                    }
                }));
            }

            await Task.WhenAll(taskList.ToArray());

            if (errorList.Any()) throw new AggregateException(errorList);

            return new ServerStartResponse {
                TaskId = agentResponse.TaskId,
                Agents = agentResponse.Agents.Select(x => new ServerStartAgentResponse {
                    Name = x.Name,
                    TaskId = x.TaskId,
                }).ToList(),
            };
        }

        public async Task<ServerStatusResponse> GetStatus(string taskId)
        {
            //...
        }

        private static async Task<TResponse> SendAsync<TRequest, TResponse>(string url, TRequest request)
        {
            //var url = NetPath.Combine(agentUrl, "/start-task");

            var serializer = new JsonSerializer();

            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            httpRequest.KeepAlive = false;

            using (var bufferStream = new MemoryStream())
            using (var bufferWriter = new StreamWriter(bufferStream)) {
                bufferWriter.AutoFlush = false;

                using (var jsonWriter = new JsonTextWriter(bufferWriter)) {
                    serializer.Serialize(jsonWriter, httpRequest);
                }

                await bufferWriter.FlushAsync();
                bufferStream.Seek(0, SeekOrigin.Begin);

                httpRequest.ContentLength = bufferStream.Length;

                using (var requestStream = await httpRequest.GetRequestStreamAsync()) {
                    await bufferStream.CopyToAsync(requestStream);
                }
            }

            using (var httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync()) {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException($"Invlaid status code {(int)httpResponse.StatusCode}:{httpResponse.StatusDescription}!");

                using (var responseStream = httpResponse.GetResponseStream()) {
                    if (responseStream == null) return default(TResponse);

                    using (var responseReader = new StreamReader(responseStream))
                    using (var jsonReader = new JsonTextReader(responseReader)) {
                        return serializer.Deserialize<TResponse>(jsonReader);
                    }
                }
            }
        }
    }
}
