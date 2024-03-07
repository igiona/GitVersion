using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace GitVersion.VersionCalculation;

[RequiresUnreferencedCode("Calls IGitVersionModule.FindAllDerivedTypes")]
public class VersionStrategyModule : IGitVersionModule
{
    //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IVersionStrategy))]
    public void RegisterTypes(IServiceCollection services)
    {
        /*
        var myType = typeof(IVersionStrategy);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies.OrderBy(x => x.FullName))
        {
            Console.WriteLine(" => {0}", assembly.FullName);

            var typeInstances = assembly
            .GetTypes()
            .OrderBy(x => x.Name);
            //.Where(x => x.IsAssignableTo(myType) && !x.IsInterface && !x.IsAbstract);

            if (!typeInstances.Any())
            {
                continue;
            }

            foreach (var typeInstance in typeInstances)
            {
                Console.WriteLine("Adding: {0}", typeInstance.Name);
                // this is the wireup that allows you to DI your instances
                services.AddSingleton(myType, typeInstance);
            }
        }*/
        //ConfiguredNextVersionVersionStrategy
        //Type[] versionStrategies = [
        //        typeof(ConfiguredNextVersionVersionStrategy),
        //        typeof(MergeMessageVersionStrategy),
        //        typeof(TaggedCommitVersionStrategy),
        //        typeof(TrackReleaseBranchesVersionStrategy),
        //        typeof(TrunkBasedVersionStrategy),
        //        typeof(VersionInBranchNameVersionStrategy)];

        var versionStrategies = IGitVersionModule.FindAllDerivedTypes<IVersionStrategy>(Assembly.GetAssembly(GetType()))
            .Where(x => x is { IsAbstract: false, IsInterface: false });

        Console.WriteLine("Strategies found: {0}", versionStrategies.Count());

        foreach (var versionStrategy in versionStrategies)
        {
            services.AddSingleton(typeof(IVersionStrategy), versionStrategy);
        }
    }
}
