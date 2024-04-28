namespace SemanticReleaseCLI.Interfaces;

public interface IGitService
{
    #region Public Methods

    Task AddFileAsync(string fileName, string? repoPath = null);

    Task CommitAsync(string message, string author, string? repoPath = null);

    Task<IReadOnlyList<GitCommit>> GetAllCommitsAsync(string? repoPath = null);

    Task<string?> GetTagAtHeadAsync(string? gitPath = null, string? workingDirectory = null);

    Task<bool> IsGitRepoAsync(string repoPath);

    Task PushAsync(string remote, string reference, string? repoPath = null);

    #endregion Public Methods
}
