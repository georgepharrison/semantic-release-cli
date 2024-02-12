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
            .Replace(Environment.NewLine, @"\n", StringComparison.Ordinal);

        string json = $"[{formattedOutput}]";

        _options.Converters.Add(new CustomDateTimeConverter("yyyy-MM-dd"));

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        return JsonSerializer.Deserialize<List<GitCommit>>(json, _options) ?? [];
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    }

    public async Task<string> GetCurrentCommit(string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("rev-parse")
                .Add("HEAD" )
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.StandardOutput;
    }

    public async Task<string?> GetTagAtHead(string? gitPath = null, string? workingDirectory = null)
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

    #endregion Public Methods
}
