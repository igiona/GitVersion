using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace GitVersion.VersionCalculation;

[RequiresUnreferencedCode("Calls IGitVersionModule.FindAllDerivedTypes")]
public class VersionStrategyModule : IGitVersionModule
{
    //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IVersionStrategy))]
    public void RegisterTypes(IServiceCollection services)
    {
        var versionStrategies = IGitVersionModule.FindAllDerivedTypes<IVersionStrategy>(Assembly.GetAssembly(GetType()))
            .Where(x => x is { IsAbstract: false, IsInterface: false });

        foreach (var versionStrategy in versionStrategies)
        {
            services.AddSingleton(typeof(IVersionStrategy), versionStrategy);
        }
    }
}
