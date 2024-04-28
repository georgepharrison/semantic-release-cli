using CliWrap;
using CliWrap.Buffered;
using SemanticReleaseCLI.Interfaces;
using System.Text.Json;

namespace SemanticReleaseCLI.Services;

public sealed class GitService(IFileSystemService fileSystemService) : IGitService
{
    #region Private Members

    private readonly IFileSystemService _fileSystemService = fileSystemService;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    #endregion Private Members

    #region Public Methods

    public async Task AddFileAsync(string fileName, string? repoPath = null)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("add")
                .Add(fileName)
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }

    public async Task CommitAsync(string message, string author, string? repoPath = null)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("commit")
                .Add("-m")
                .Add(message)
                .Add($"--author={author}")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }

    public async Task<IReadOnlyList<GitCommit>> GetAllCommitsAsync(string? repoPath = null)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("log")
                .Add("--format={ \"Id\": \"%h\", \"ParentId\": \"%p\", \"AuthorDate\": \"%ai\", \"AuthorName\": \"%an\", \"AuthorEmail\": \"%ae\", \"RefNames\": \"%d\", \"Subject\": \"%s\", \"Body\": \"%b\" }")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        string formattedOutput = result.StandardOutput[..^1]
            .Replace($"}}{Environment.NewLine}{{", "},{", StringComparison.Ordinal)
            .Replace($"}}\r\n{{", "},{", StringComparison.Ordinal)
            .Replace($"}}\r{{", "},{", StringComparison.Ordinal)
            .Replace($"}}\n{{", "},{", StringComparison.Ordinal)
            .Replace(Environment.NewLine, @"\r\n", StringComparison.Ordinal)
            .Replace("\n", @"\r\n", StringComparison.Ordinal)
            .Replace("\r\r\n", @"\r\n", StringComparison.Ordinal);

        string json = $"[{formattedOutput}]";

        _options.Converters.Add(new CustomDateTimeConverter("yyyy-MM-dd"));

        return JsonSerializer.Deserialize<List<GitCommit>>(json, _options) ?? [];
    }

    public async Task<string?> GetTagAtHeadAsync(string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("tag")
                .Add("--points-at")
                .Add("HEAD")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        if (!string.IsNullOrEmpty(result.StandardError))
        {
            return null;
        }

        return string.IsNullOrEmpty(result.StandardOutput) ? null : result.StandardOutput[..^1];
    }

    public async Task<bool> IsGitRepoAsync(string repoPath)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath)
            .WithArguments(args => args
                .Add("rev-parse")
                .Add("--git-dir")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode is 0;
    }

    public async Task PushAsync(string remote, string reference, string? repoPath = null)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("push")
                .Add(remote)
                .Add(reference)
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }

    #endregion Public Methods
}
