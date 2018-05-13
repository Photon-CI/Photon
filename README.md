# Photon
> A platform for packaging and deploying .NET-based automation scripts to provide a complete pipeline-as-code solution for building, testing, and deploying projects.

Photon packages your custom .NET library with your applications, allowing users to build, test, and deploy their applications with the full power of .NET! By managing your custom libraries alongside your projects, you can gain full support for branch versioning of your automated tasks. This allows users to make structural project changes that alter their pipelines, without affecting other branches of code!

Since standard .NET libraries are used, you can leverage the full user experience of Visual Studio when editing your pipelines. By decorating your scripts and tasks with test attributes, you can even make your tasks directly runnable and debuggable using your favorite testing platform.

Visit the [Official Website](http://www.photon.ci/) to download releases and view the documentation.

## Usage example

**Scripts** `: IScript`

Scripts are run on the Server, and are primarily used to delegate work to agents. In the below script, we register all agents matching the roles `Configuration.Roles.Deploy.Web` and `Configuration.Roles.Deploy.Service`, which are pre-defined string constants.

```c#
public class DeployScript : IScript
{
    public async Task<ScriptResult> RunAsync(ScriptContext context)
    {
        var agents = context.RegisterAgents(
            "role.deploy.web",
            "role.deploy.Service");

        try {
            await agents.InitializeAsync();

            // Unpack Applications
            await agents.RunTasksAsync(
                nameof(UnpackPhotonSampleWeb),
                nameof(UnpackPhotonSampleService));

            // Update Applications
            await agents.RunTasksAsync(
                nameof(UpdatePhotonSampleWeb),
                nameof(UpdatePhotonSampleService));
        }
        finally {
            await agents.ReleaseAllAsync();
        }
    }
}

```

Tasks are run on Agents. By specifying the `[Roles]` attribute, tasks can be limited to Agents matching those roles. This allows multiple tasks to be executed simultaneously while only executing on agents matching their role.

```c#
[Roles(Configuration.Roles.Deploy.Web)]
internal class UnpackPhotonSampleWeb : IDeployTask
{
    public IAgentDeployContext Context {get; set;}

    public async Task<TaskResult> RunAsync()
    {
        // Get the versioned application path
        var applicationPath = Context.GetApplicationDirectory(Configuration.Apps.Web.AppName, Context.ProjectPackageVersion);

        // Download Package to temp file
        var packageFilename = await Context.PullApplicationPackageAsync(Configuration.Apps.Web.PackageId, Context.ProjectPackageVersion);

        // Unpackage contents to application path
        await ApplicationPackageTools.UnpackAsync(packageFilename, applicationPath);
    }
}

```

```c#
[Roles(Configuration.Roles.Deploy.Web)]
internal class UpdatePhotonSampleWeb : IDeployTask
{
    public IAgentDeployContext Context {get; set;}

    public async Task RunAsync()
    {
        // Get the versioned application path
        var applicationPath = Context.GetApplicationDirectory(Configuration.Apps.Web.AppName, Context.ProjectPackageVersion);

        using (var iis = new IISTools(Context)) {
            // Configure and start AppPool
            iis.ApplicationPool.Configure(Configuration.AppPoolName, pool => {
                pool.AutoStart = true;
                pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                pool.ManagedRuntimeVersion = "v4.0";

                if (pool.State == ObjectState.Stopped)
                    pool.Start();
            });

            // Configure and start Website
            iis.WebSite.Configure("Photon Web", 8086, site => {
                site.ApplicationDefaults.ApplicationPoolName = Configuration.AppPoolName;
                site.ServerAutoStart = true;

                // Set Bindings
                site.Bindings.Clear();
                site.Bindings.Add("*:8086:localhost", "http");

                // Update Virtual Path
                site.Applications[0]
                    .VirtualDirectories["/"]
                    .PhysicalPath = applicationPath;

                if (site.State == ObjectState.Stopped)
                    site.Start();
            });
        }
    }
}
```

## Release History

* 0.0.1
    * Initial Release; Work in progress.

## Meta

Joshua Miller - [null511@GitHub](https://github.com/null511) – <mailto:null511@outlook.com>

Distributed under the MIT license. See ``LICENSE`` for more information.

## Contributing

1. Fork it (<https://github.com/Photon-CI/Photon/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
