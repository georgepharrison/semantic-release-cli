using System.Diagnostics.CodeAnalysis;

namespace SemanticReleaseCLI;

public interface IFileSystemService
{
    #region Public Methods

    string GetCurrentDirectory();

    bool TryGetFileContents(string fileName, [MaybeNullWhen(false)] out string? contents);

    Task WriteAllTextAsync(string fileName, string contents);

    Task<string> WriteAllTextAsync(string fileName, string contents, params string[] paths);

    #endregion Public Methods
}
