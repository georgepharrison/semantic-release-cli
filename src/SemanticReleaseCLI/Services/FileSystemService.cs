using System.Diagnostics.CodeAnalysis;

namespace SemanticReleaseCLI;

public sealed class FileSystemService : IFileSystemService
{
    #region Public Methods

    public string GetCurrentDirectory()
        => Directory.GetCurrentDirectory();
 
    public bool TryGetFileContents(string fileName, [MaybeNullWhen(false)] out string? contents)
    {
        if (!File.Exists(fileName))
        {
            contents = null;
            
            return false;
        }

        contents = File.ReadAllText(fileName);

        return true;
    }

    #endregion Public Methods
}
