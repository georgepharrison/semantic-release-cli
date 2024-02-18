namespace SemanticReleaseCLI.Interfaces;

public interface IReleaseCliService
{
    #region Public Methods

    Task CreateAsync(string name, string tagName, string commitReference, string description);

    #endregion Public Methods
}
