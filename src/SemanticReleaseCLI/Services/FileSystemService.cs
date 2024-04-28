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

    public async Task WriteAllTextAsync(string fileName, string contents)
        => await File.WriteAllTextAsync(fileName, contents);

    public async Task<string> WriteAllTextAsync(string fileName, string contents, params string[] paths)
    {
        string fileDirectory = Path.Combine(paths);

        Directory.CreateDirectory(fileDirectory);

        fileName = Path.Combine(fileDirectory, fileName);

        await WriteAllTextAsync(fileName, contents);

        return fileName;
    }

    #endregion Public Methods
}
