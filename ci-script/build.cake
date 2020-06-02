#addin "Cake.Docker"
#addin "Cake.Http"
#addin nuget:?package=Newtonsoft.Json
#addin nuget:?package=Cake.DoInDirectory

var target = Argument("target", "Default");
var latestTag = Argument("latestTag", "latest");
var helmCommand = "upgrade --install --force";
var projectName = "helloworld";
var registryName = "abideev";
var DockerToken = Argument("token", "");

if(string.IsNullOrEmpty(DockerToken)) {
	throw new Exception("Could not get dockerToken");
}

Task("Build")
.Does(() => {
    DoInDirectory("../sources/", () => {
       DockerBuild(new DockerImageBuildSettings {Tag  = new [] {$"{projectName}:latest"}}, "./");
    });
});

Task("Push-Image")
    .IsDependentOn("Build")
    .Does(() =>
{   
    Information($"Tag image {projectName}:latest with {registryName}/{projectName}:{latestTag}");
    DockerLogin(new DockerRegistryLoginSettings{Password=$"{DockerToken}",Username="abideev"});
    DockerTag($"{projectName}:latest", $"{registryName}/{projectName}:{latestTag}");
    DockerPush($"{registryName}/{projectName}:{latestTag}");
});

Task("Deploy")
    .IsDependentOn("Push-Image")
    .Does(()=>
{
        Information($"Deploy to k8s cluster:");
        DoInDirectory($"../ci-deploy/{projectName}", () => {
            var processArgumentBuilder = new ProcessArgumentBuilder();
            processArgumentBuilder.Append($"{helmCommand} {projectName}")
                                  .Append($"-f values.yaml");
            processArgumentBuilder.Append($"--set image.tag={latestTag}");            
            processArgumentBuilder.Append($"--set image.repository={registryName}/{projectName}");
            processArgumentBuilder.Append("--atomic --timeout 60");
            processArgumentBuilder.Append(".");

            var processSettings = new ProcessSettings 
            {
                Arguments = processArgumentBuilder
            };
            var exitCode = StartProcess("/usr/local/bin/helm", processSettings);
            if (exitCode != 0)
				Console.WriteLine ("Helm failed with exit code {0}", exitCode);
        });
});

Task("Default")
    .IsDependentOn("Deploy");

RunTarget(target);
