using System.Diagnostics.CodeAnalysis;
using GitVersion.Common;
using GitVersion.Configuration;
using GitVersion.Extensions;

namespace GitVersion.VersionCalculation;

/// <summary>
/// Active only when the branch is marked as IsDevelop.
/// Two different algorithms (results are merged):
/// <para>
/// Using <see cref="VersionInBranchNameVersionStrategy"/>:
/// Version is that of any child branches marked with IsReleaseBranch (except if they have no commits of their own).
/// BaseVersionSource is the commit where the child branch was created.
/// Always increments.
/// </para>
/// <para>
/// Using <see cref="TaggedCommitVersionStrategy"/>:
/// Version is extracted from all tags on the <c>main</c> branch which are valid.
/// BaseVersionSource is the tag's commit (same as base strategy).
/// Increments if the tag is not the current commit (same as base strategy).
/// </para>
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal class TrackReleaseBranchesVersionStrategy(IRepositoryStore repositoryStore, GitVersionContext versionContext)
    : VersionStrategyBase(versionContext)
{
    private readonly VersionInBranchNameVersionStrategy releaseVersionStrategy = new(repositoryStore, versionContext);

    private readonly IRepositoryStore repositoryStore = repositoryStore.NotNull();

    public override IEnumerable<BaseVersion> GetBaseVersions(EffectiveBranchConfiguration configuration)
    {
        if (!Context.Configuration.VersionStrategy.HasFlag(VersionStrategies.TrackReleaseBranches))
            return [];

        return configuration.Value.TracksReleaseBranches ? ReleaseBranchBaseVersions() : [];
    }

    private IEnumerable<BaseVersion> ReleaseBranchBaseVersions()
    {
        var releaseBranchConfig = Context.Configuration.GetReleaseBranchConfiguration();
        if (releaseBranchConfig.Count == 0)
            return [];

        var releaseBranches = this.repositoryStore.GetReleaseBranches(releaseBranchConfig);

        return releaseBranches
            .SelectMany(GetReleaseVersion)
            .Select(baseVersion =>
            {
                // Need to drop branch overrides and give a bit more context about
                // where this version came from
                var source1 = "Release branch exists -> " + baseVersion.Source;
                return new BaseVersion(source1,
                    baseVersion.ShouldIncrement,
                    baseVersion.GetSemanticVersion(),
                    baseVersion.BaseVersionSource,
                    null);
            })
            .ToList();
    }

    private IEnumerable<BaseVersion> GetReleaseVersion(IBranch releaseBranch)
    {
        // Find the commit where the child branch was created.
        var baseSource = this.repositoryStore.FindMergeBase(releaseBranch, Context.CurrentBranch);
        var effectiveBranchConfiguration = Context.Configuration.GetEffectiveBranchConfiguration(releaseBranch);
        return this.releaseVersionStrategy
            .GetBaseVersions(effectiveBranchConfiguration)
            .Select(b => new BaseVersion(b.Source, true, b.GetSemanticVersion(), baseSource, b.BranchNameOverride));
    }
}
