using DotLiquid;

namespace SemanticReleaseCLI;

public interface IRepositoryService
{
    #region Public Methods

    Template GetChangeLogTemplate(string repoPath);

    Task<IReadOnlyList<Release>> GetReleasesAsync(string repoPath);

    IReadOnlyList<dynamic> GetTemplateData(string repoPath, Release release);

    IReadOnlyList<dynamic> GetTemplateData(string repoPath, IEnumerable<Release> releases);

    #endregion Public Methods
}
