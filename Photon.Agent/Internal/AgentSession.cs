using log4net;
using Photon.Framework;
using Photon.Framework.Messages;
using Photon.Library;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSession : IReferenceItem, IDisposable
    {
        private readonly Lazy<ILog> _log;
        private readonly DateTime utcCreated;
        private bool isReleased;

        public string Id {get;}
        protected AgentSessionDomain Domain {get;}
        //public TaskContext Context {get;}
        public string WorkDirectory {get; set;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        protected ILog Log => _log.Value;


        public AgentSession()
        {
            Id = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
        }

        public void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            Domain?.Dispose();
        }

        public async Task RunAsync()
        {
            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.Job.Assembly);

            if (!File.Exists(assemblyFilename)) {
                errorList.Value.Add(new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!"));
                Context.Output.AppendLine($"The assembly file '{assemblyFilename}' could not be found!");
                //throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");
                abort = true;
            }

            if (!abort) {
                try {
                    Domain.Initialize(assemblyFilename);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script initialization failed! [{Id}]", error));
                    //Log.Error($"Script initialization failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while initializing the script! {error.Message} [{Id}]");
                    abort = true;
                }
            }

            if (!abort) {
                try {
                    var result = await Domain.RunScript(Context);
                    if (!result.Successful) throw new ApplicationException(result.Message);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script execution failed! [{Id}]", error));
                    //Log.Error($"Script execution failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script! {error.Message} [{Id}]");
                }
            }
        }

        public async Task ReleaseAsync()
        {
            Domain?.Dispose();

            if (!isReleased) {
                var workDirectory = WorkDirectory;
                try {
                    await Task.Run(() => FileUtils.DestoryDirectory(workDirectory));
                }
                catch (AggregateException errors) {
                    errors.Flatten().Handle(e => {
                        if (e is IOException ioError) {
                            Log.Warn(errors.Message);
                            return true;
                        }

                        return false;
                    });
                }

                isReleased = true;
            }
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        public virtual void PrepareWorkDirectory()
        {
            Directory.CreateDirectory(Context.WorkDirectory);
        }

        public async Task RunTask(string taskName, string jsonData = null)
        {
            var context = new AgentContext();

            await Domain.RunTask(taskName, context);
        }

        private void DownloadPackage(string packageName, string version, string outputDirectory)
        {
            //...
            throw new NotImplementedException();
        }
    }
}
