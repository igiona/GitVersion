using System.Diagnostics.CodeAnalysis;
using GitVersion.Agents;
using GitVersion.Configuration;
using GitVersion.Extensions;
using GitVersion.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GitVersion;

//[RequiresUnreferencedCode("Calls many dyn stuff")]
internal class Program
{
    private readonly Action<IServiceCollection>? overrides;

    internal Program(Action<IServiceCollection>? overrides = null) => this.overrides = overrides;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Program))]
    [RequiresUnreferencedCode("Calls many dyn stuff")]
    private static async Task Main(string[] args) => await new Program().RunAsync(args);

    [RequiresUnreferencedCode("Calls many dyn stuff")]
    internal Task RunAsync(string[] args) => CreateHostBuilder(args).Build().RunAsync();

    [RequiresUnreferencedCode("Calls many dyn stuff")]
    private IHostBuilder CreateHostBuilder(string[] args) =>
        new HostBuilder()
            .ConfigureAppConfiguration((_, configApp) => configApp.AddCommandLine(args))
            .ConfigureServices((_, services) =>
            {
                services.AddModule(new GitVersionCoreModule());
                services.AddModule(new GitVersionLibGit2SharpModule());
                services.AddModule(new GitVersionBuildAgentsModule());
                services.AddModule(new GitVersionConfigurationModule());
                services.AddModule(new GitVersionOutputModule());
                services.AddModule(new GitVersionAppModule());

                services.AddSingleton(sp =>
                {
                    var arguments = sp.GetRequiredService<IArgumentParser>().ParseArguments(args);
                    var gitVersionOptions = arguments.ToOptions();
                    return Options.Create(gitVersionOptions);
                });

                this.overrides?.Invoke(services);
                services.AddHostedService<GitVersionApp>();
            })
            .UseConsoleLifetime();
}
