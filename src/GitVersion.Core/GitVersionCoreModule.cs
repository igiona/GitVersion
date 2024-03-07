using System.Diagnostics.CodeAnalysis;
using GitVersion.Common;
using GitVersion.Core;
using GitVersion.Extensions;
using GitVersion.VersionCalculation;
using GitVersion.VersionCalculation.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GitVersion;

[RequiresUnreferencedCode("Calls VersionCalculationModule")]
public class GitVersionCoreModule : IGitVersionModule
{
    public void RegisterTypes(IServiceCollection services)
    {
        Console.WriteLine("Adding GitVersionCoreModule");
        services.AddSingleton<IGitVersionCache, GitVersionCache>();

        services.AddSingleton<IGitVersionCalculateTool, GitVersionCalculateTool>();

        services.AddSingleton<IGitPreparer, GitPreparer>();
        services.AddSingleton<IRepositoryStore, RepositoryStore>();
        services.AddSingleton<ITaggedSemanticVersionRepository, TaggedSemanticVersionRepository>();
        services.AddSingleton<IBranchRepository, BranchRepository>();

        services.AddSingleton<IGitVersionContextFactory, GitVersionContextFactory>();

        services.AddSingleton(sp =>
        {
            Console.WriteLine("Adding GitVersionContext");
            var options = sp.GetRequiredService<IOptions<GitVersionOptions>>();
            var contextFactory = sp.GetRequiredService<IGitVersionContextFactory>();
            return contextFactory.Create(options.Value);
        });

        services.AddModule(new GitVersionCommonModule());
        services.AddModule(new VersionCalculationModule());
    }
}
