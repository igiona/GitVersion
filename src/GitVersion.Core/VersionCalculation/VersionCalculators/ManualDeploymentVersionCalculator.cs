using GitVersion.Common;
using GitVersion.Logging;

namespace GitVersion.VersionCalculation;

internal sealed class ManualDeploymentVersionCalculator(ILog log, IRepositoryStore repositoryStore, GitVersionContext versionContext)
    : VersionCalculatorBase(log, repositoryStore, versionContext), IDeploymentModeCalculator
{
    public SemanticVersion Calculate(SemanticVersion semanticVersion, ICommit? baseVersionSource)
    {
        using (this.log.IndentLog("Using manual deployment workflow to calculate the incremented version."))
        {
            Console.WriteLine("ManDep");
            return CalculateInternal(semanticVersion, baseVersionSource);
        }
    }

    private SemanticVersion CalculateInternal(SemanticVersion semanticVersion, ICommit? baseVersionSource)
    {
        var buildMetaData = CreateVersionBuildMetaData(baseVersionSource);

        return new SemanticVersion(semanticVersion)
        {
            BuildMetaData = buildMetaData
        };
    }
}
