# Photon
> A platform for packaging and deploying .NET-based automation scripts to provide a complete pipeline-as-code solution for building, testing, and deploying projects.

<!--[![NPM Version][npm-image]][npm-url]
[![Build Status][travis-image]][travis-url]
[![Downloads Stats][npm-downloads]][npm-url]-->

Photon packages your custom .NET library with your applications, allowing users to build, test, and deploy their applications with the full power of .NET! By managing your custom libraries alongside your projects, you can gain full support for branch versioning of your automated tasks. This allows users to make structural project changes that alter their pipelines, without affecting other branches of code!

Since standard .NET libraries are used, you can leverage the full user experience of Visual Studio when editing your pipelines. By decorating your scripts and tasks with test attributes, you can even make your tasks directly runnable and debuggable using your favorite testing platform.

<!--![](header.png)-->

## Installation

Though Photon is primarily designed for Windows, it is fully compatible with the [MONO](http://www.mono-project.com) Runtime; Which currently† supports Linux, OS X, BSD, and Windows. It can even be run on the [Raspberry Pi](https://www.raspberrypi.org/)!  
† _04/2018_

**Photon Server**  
To begin using Photon, you will first need to create and define your primary Server responsible for managing all application packages and delegating Agent operations.

**Photon Agent**  
Agents can be used to perform Tasks remotely, such as Building, Testing, and Deploying of packages; and should be installed on the remote servers you plan to perform work/deployments upon.

**Photon CLI**
An optional command-line executable for creating packages, executing scripts, and calling into the Photon Server HTTP API.

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

        return ScriptResult.Ok();
    }
}

```

Tasks are run on Agents. By specifying the `[Roles]` attribute, tasks can be limited to Agents matching those roles. This allows multiple tasks to be executed simultaneously while only executing on agents matching their role.

```c#
[Roles(Configuration.Roles.Deploy.Web)]
class UnpackPhotonSampleWeb : ITask
{
    public TaskResult Run(TaskContext context)
    {
        // Download package to working directory
        var packageFilename = context.DownloadPackage("photon.sample.web", context.ReleaseVersion, context.WorkDirectory);

        // Get the versioned application path
        var applicationPath = context.GetApplicationDirectory(Configuration.Apps.Web, context.ReleaseVersion);

        // Unpackage contents to application path
        PackageTools.Unpackage(packageFilename, applicationPath);

        return TaskResult.Ok();
    }
}
```

```c#
[Roles(Configuration.Roles.Deploy.Web)]
class UpdatePhotonSampleWeb : ITask
{
    public async Task<TaskResult> RunAsync(TaskContextBase context)
    {
        // Get the versioned application path
        var applicationPath = context.GetApplicationDirectory(Configuration.Apps.Web, context.ReleaseVersion);

        var iis = new IISTools();

        iis.ConfigureAppPool(Configuration.AppPoolName, pool => {
            // Configure AppPool
            pool.AutoStart = true;
            pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            pool.ManagedRuntimeVersion = "v4.0";

            // Start AppPool
            if (pool.State == ObjectState.Stopped)
                pool.Start();
        });

        iis.ConfigureWebSite("Photon Web", site => {
            // Configure Website
            site.ApplicationDefaults.ApplicationPoolName = Configuration.AppPoolName;
            site.ServerAutoStart = true;

            // Set Bindings
            site.Bindings.Clear();
            site.Bindings.Add("*:80:localhost", "http");

            // Update Virtual Path
            site.Applications[0]
                .VirtualDirectories["/"]
                .PhysicalPath = applicationPath;

            // Start Website
            if (site.State == ObjectState.Stopped)
                site.Start();
        });

        return TaskResult.Ok();
    }
}
```

_For more examples and usage, please refer to the [Wiki][wiki]._

## Release History

* 0.0.1
    * Initial Release; Work in progress.

## Meta

Joshua Miller – null511@outlook.com

Distributed under the MIT license. See ``LICENSE`` for more information.

[https://github.com/null511](https://github.com/null511)

## Contributing

1. Fork it (<https://github.com/null511/photon/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

<!-- Markdown link & img dfn's -->
[wiki]: https://github.com/null511/Photon/wiki
