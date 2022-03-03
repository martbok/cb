//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////
#module "nuget:?package=Cake.DotNetTool.Module&version=0.4.0"
#tool "dotnet:?package=GitVersion.Tool&version=5.8.2"
#addin "nuget:?package=Cake.Docker&version=0.11.0"

//////////////////////////////////////////////////////////////////////
// NAMESPACES
//////////////////////////////////////////////////////////////////////
using System.Linq;
using SystemTask = System.Threading.Tasks.Task;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
GitVersion gitVersionInfo;
string nugetVersion;

// Docker CI settings
var dockerComposeFiles = new [] { "docker-compose.yml" };
var dockerUsed = false;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });

   nugetVersion = gitVersionInfo.NuGetVersion;

    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(ctx =>
{
    if (dockerUsed)
    {
        // Bring down test environment
        DockerComposeDown(new DockerComposeDownSettings {
            Files = dockerComposeFiles
        });
    }

   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("__DockerComposeUp")
    .Does(() =>
    {
        dockerUsed = true;

        // Bring up test environment
        var settings = new DockerComposeUpSettings
        {
            Build = true,
            DetachedMode = true,
            Files = dockerComposeFiles
        };
        DockerComposeUp(settings);
    });

Task("Build")
   .IsDependentOn("__DockerComposeUp");

Task("Default").IsDependentOn("Build");

RunTarget(target);