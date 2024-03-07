using System.Diagnostics.CodeAnalysis;
using GitVersion.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GitVersion;

public interface IGitVersionModule
{
    void RegisterTypes(IServiceCollection services);

    [RequiresUnreferencedCode("Searches the provided type T in assembly via reflection.")]
    static IEnumerable<Type> FindAllDerivedTypes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Assembly? assembly)
    {
        assembly.NotNull();

        var derivedType = typeof(T);
        return assembly.GetTypes().Where(t => t != derivedType && derivedType.IsAssignableFrom(t));
    }
}
