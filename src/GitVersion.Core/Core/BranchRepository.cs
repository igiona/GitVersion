using GitVersion.Configuration;
using GitVersion.Extensions;

namespace GitVersion.Core;

internal sealed class BranchRepository(IGitRepository gitRepository) : IBranchRepository
{
    private readonly IGitRepository gitRepository = gitRepository.NotNull();

    public IEnumerable<IBranch> GetMainBranches(IGitVersionConfiguration configuration, params IBranch[] excludeBranches)
        => GetBranches(configuration, [.. excludeBranches], configuration => configuration.IsMainBranch == true);

    public IEnumerable<IBranch> GetReleaseBranches(IGitVersionConfiguration configuration, params IBranch[] excludeBranches)
        => GetBranches(configuration, [.. excludeBranches], configuration => configuration.IsReleaseBranch == true);

    private IEnumerable<IBranch> GetBranches(
        IGitVersionConfiguration configuration, HashSet<IBranch> excludeBranches, Func<IBranchConfiguration, bool> predicate)
    {
        predicate.NotNull();

        foreach (var branch in this.gitRepository.Branches)
        {
            if (!excludeBranches.Contains(branch))
            {
                var branchConfiguration = configuration.GetBranchConfiguration(branch.Name);
                if (predicate(branchConfiguration))
                {
                    yield return branch;
                }
            }
        }
    }
}
