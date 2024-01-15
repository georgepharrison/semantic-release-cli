namespace SemanticReleaseCLI.Interfaces;

public interface IGitService
{
    #region Public Methods

    Task AmendCommit(string? gitPath = null, string? workingDirectory = null);

    Task CreateAndPushTag(string tag, string? gitPath = null, string? workingDirectory = null);

    Task DeleteTags(string pattern, string? gitPath = null, string? workingDirectory = null);

    Task<int> GetCommitCount(string since, string until, string? gitPath = null, string? workingDirectory = null);

    Task<string> GetCommitDate(string? format = "%cs", string? reference = null, string? gitPath = null, string? workingDirectory = null);

    Task<string> GetCurrentCommit(string? gitPath = null, string? workingDirectory = null);

    IAsyncEnumerable<string> GetLogs(string? startIndex = null, string? endIndex = null, string? gitPath = null, string? workingDirectory = null);

    Task<string?> GetTagAtHead(string? gitPath = null, string? workingDirectory = null);

    Task<string?> GetTagBeforeHead(string? gitPath = null, string? workingDirectory = null);

    IAsyncEnumerable<string> GetTags(string? sort = "committerdate", string? gitPath = null, string? workingDirectory = null);

    Task<bool> IsGitRepoAsync(string? repoPath = null);

    IAsyncEnumerable<string> SearchLogs(string grep, string? regexType = null, string? startIndex = null, string? endIndex = null, string? gitPath = null, string? workingDirectory = null);

    Task StageFile(string file, string? gitPath = null, string? workingDirectory = null);

    #endregion Public Methods
}