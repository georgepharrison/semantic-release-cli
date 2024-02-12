namespace SemanticReleaseCLI.Interfaces;

public interface IGitService
{
    #region Public Methods

    Task<IReadOnlyList<GitCommit>> GetAllCommitsAsync(string? repoPath = null);

    Task<string?> GetTagAtHead(string? gitPath = null, string? workingDirectory = null);

    Task<bool> IsGitRepoAsync(string repoPath);

    #endregion Public Methods
}