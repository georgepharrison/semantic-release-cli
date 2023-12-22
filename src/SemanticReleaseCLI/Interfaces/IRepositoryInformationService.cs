namespace SemanticReleaseCLI.Interfaces;

public interface IRepositoryInformationService
{
    #region Public Methods

    Task CreateChangeLog(string? workingDirectory = null);

    Task CreateRelease(string? workingDirectory = null);

    Task<string> GetCommitTag(string? workingDirectory = null);

    #endregion Public Methods
}