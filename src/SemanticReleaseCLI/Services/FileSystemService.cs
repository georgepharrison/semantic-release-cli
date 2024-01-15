namespace SemanticReleaseCLI;

public class FileSystemService : IFileSystemService
{
    public string GetCurrentDirectory()
        => Directory.GetCurrentDirectory();
}
