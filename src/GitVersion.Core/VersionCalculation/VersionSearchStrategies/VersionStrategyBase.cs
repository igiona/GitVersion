using System.Diagnostics.CodeAnalysis;
using GitVersion.Configuration;
using GitVersion.Extensions;

namespace GitVersion.VersionCalculation;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public abstract class VersionStrategyBase(GitVersionContext versionContext) : IVersionStrategy
{
    private readonly GitVersionContext versionContext = versionContext.NotNull();

    protected GitVersionContext Context => this.versionContext;

    public abstract IEnumerable<BaseVersion> GetBaseVersions(EffectiveBranchConfiguration configuration);
}
