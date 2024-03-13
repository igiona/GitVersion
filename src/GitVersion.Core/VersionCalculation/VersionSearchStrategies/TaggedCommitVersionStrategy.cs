using System.Diagnostics.CodeAnalysis;
using GitVersion.Configuration;
using GitVersion.Core;
using GitVersion.Extensions;

namespace GitVersion.VersionCalculation;

/// <summary>
/// Version is extracted from all tags on the branch which are valid, and not newer than the current commit.
/// BaseVersionSource is the tag's commit.
/// Increments if the tag is not the current commit.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal sealed class TaggedCommitVersionStrategy(ITaggedSemanticVersionRepository taggedSemanticVersionRepository, GitVersionContext versionContext)
    : VersionStrategyBase(versionContext)
{
    private readonly ITaggedSemanticVersionRepository taggedSemanticVersionRepository = taggedSemanticVersionRepository.NotNull();

    public override IEnumerable<BaseVersion> GetBaseVersions(EffectiveBranchConfiguration configuration)
        => !Context.Configuration.VersionStrategy.HasFlag(VersionStrategies.TaggedCommit) ? []
        : GetTaggedSemanticVersions(configuration).Select(CreateBaseVersion);

    private IEnumerable<SemanticVersionWithTag> GetTaggedSemanticVersions(EffectiveBranchConfiguration configuration)
    {
        configuration.NotNull();

        var label = configuration.Value.GetBranchSpecificLabel(Context.CurrentBranch.Name, null);
        var taggedSemanticVersions = taggedSemanticVersionRepository
            .GetAllTaggedSemanticVersions(Context.Configuration, configuration.Value, Context.CurrentBranch, label, Context.CurrentCommit.When)
            .SelectMany(element => element)
            .Distinct().ToArray();

        foreach (var semanticVersion in taggedSemanticVersions)
        {
            yield return semanticVersion;
        }
    }

    private static BaseVersion CreateBaseVersion(SemanticVersionWithTag semanticVersion)
    {
        var tagCommit = semanticVersion.Tag.Commit;
        return new(
             $"Git tag '{semanticVersion.Tag.Name.Friendly}'", true, semanticVersion.Value, tagCommit, null
         );
    }
}
