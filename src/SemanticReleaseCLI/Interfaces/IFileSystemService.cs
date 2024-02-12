using System.Diagnostics.CodeAnalysis;

namespace SemanticReleaseCLI;

public interface IFileSystemService
{
    #region Public Methods

    string GetCurrentDirectory();

    bool TryGetFileContents(string fileName, [MaybeNullWhen(false)] out string? contents);
 
    #endregion Public Methods
}
