// Ignore Spelling: grep

using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using CliWrap.EventStream;
using SemanticReleaseCLI.Extensions;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;

namespace SemanticReleaseCLI.Services;

public class GitService(IFileSystemService fileSystemService) : IGitService
{
    #region Private Members

    private readonly IFileSystemService _fileSystemService = fileSystemService;

    #endregion Private Members

    #region Public Methods

    public async Task<bool> IsGitRepoAsync(string? repoPath = null)
    {
        BufferedCommandResult result = await Cli.Wrap("git")
            .WithWorkingDirectory(repoPath ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("rev-parse")
                .Add("--git-dir")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode is 0;
    }

    public async Task AmendCommit(string? gitPath = null, string? workingDirectory = null)
    {
        await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "commit", "--amend", "--no-edit" })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    public async Task CreateAndPushTag(string tag, string? gitPath = null, string? workingDirectory = null)
    {
        await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "tag", tag })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "push", "origin", tag })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    public async Task DeleteTags(string pattern, string? gitPath = null, string? workingDirectory = null)
    {
        var foo = _fileSystemService.GetCurrentDirectory();

        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "tag", "--list", pattern })
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        string[] tagsToRemove = result.StandardOutput.SplitToLines().ToArray();

        if (tagsToRemove.Length is 0)
        {
            AnsiConsole.WriteLine("No tags found for removal");
        }
        else
        {
            AnsiConsole.WriteLine($"Tags found for removal: {string.Join(", ", tagsToRemove)}");

            Command cmd = Cli.Wrap(gitPath ?? "git")
                    .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
                    .WithArguments(args =>
                    {
                        args.Add("push")
                            .Add("origin")
                            .Add("-d");

                        foreach (string tag in tagsToRemove)
                        {
                            args.Add(tag);
                        }
                    })
                    .WithValidation(CommandResultValidation.None);

            await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        AnsiConsole.WriteLine("Removing tags from origin");
                        break;

                    case StandardOutputCommandEvent stdOut:
                        AnsiConsole.WriteLine(stdOut.Text);
                        break;

                    case StandardErrorCommandEvent stdErr:
                        AnsiConsole.WriteLine(stdErr.Text);
                        break;

                    default:
                        break;
                }
            }

            cmd = Cli.Wrap(gitPath ?? "git")
                .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
                .WithArguments(args =>
                {
                    args.Add("tag")
                        .Add("-d");

                    foreach (string tag in tagsToRemove)
                    {
                        args.Add(tag);
                    }
                })
                .WithValidation(CommandResultValidation.None);

            await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        AnsiConsole.WriteLine("Removing local tags");
                        break;

                    case StandardOutputCommandEvent stdOut:
                        AnsiConsole.WriteLine(stdOut.Text);
                        break;

                    case StandardErrorCommandEvent stdErr:
                        AnsiConsole.WriteLine(stdErr.Text);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public async Task<int> GetCommitCount(string since, string until, string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("rev-list")
                .Add("--count")
                .Add($"--since={since}")
                .Add($"--until={until}")
                .Add("HEAD")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return Convert.ToInt32(result.StandardOutput);
    }

    public async Task<string> GetCommitDate(string? format = "%cs", string? reference = null, string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args =>
            {
                args.Add("log");

                if (!string.IsNullOrEmpty(reference))
                {
                    args.Add(reference);
                }

                args.Add("-1");
                args.Add($"--format={format}");
            })
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.StandardOutput[..^1];
    }

    public async Task<string> GetCurrentCommit(string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "rev-parse", "HEAD" })
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.StandardOutput;
    }

    public async IAsyncEnumerable<string> GetLogs(string? startIndex = null, string? endIndex = null, string? gitPath = null, string? workingDirectory = null)
    {
        int skip = 0;

        while (true)
        {
            BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
                .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
                .WithArguments(args =>
                {
                    args.Add("log");

                    if (!string.IsNullOrEmpty(startIndex) && !string.IsNullOrEmpty(endIndex))
                    {
                        args.Add($"{startIndex}..{endIndex}");
                    }
                    else if (!string.IsNullOrEmpty(startIndex))
                    {
                        args.Add($"{startIndex}..HEAD");
                    }
                    else if (!string.IsNullOrEmpty(endIndex))
                    {
                        args.Add(endIndex);
                    }

                    args.Add($"--skip={skip++}");
                    args.Add("-n1");
                })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                yield break;
            }

            yield return result.StandardOutput;
        }
    }

    public async Task<string?> GetTagAtHead(string? gitPath = null, string? workingDirectory = null)
    {
        BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "tag", "--points-at", "HEAD" })
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        if (!string.IsNullOrEmpty(result.StandardError))
        {
            return null;
        }

        return string.IsNullOrEmpty(result.StandardOutput) ? null : result.StandardOutput[..^1];
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Tag wasn't found")]
    public async Task<string?> GetTagBeforeHead(string? gitPath = null, string? workingDirectory = null)
    {
        try
        {
            BufferedCommandResult result = await Cli.Wrap(gitPath ?? "git")
                .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
                .WithArguments(new[] { "describe", "--tags", "--abbrev=0", "HEAD^" })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (!string.IsNullOrEmpty(result.StandardError))
            {
                return null;
            }

            return string.IsNullOrEmpty(result.StandardOutput) ? null : result.StandardOutput[..^1];
        }
        catch
        {
            return null;
        }
    }

    public async IAsyncEnumerable<string> GetTags(string? sort = "committerdate", string? gitPath = null, string? workingDirectory = null)
    {
        Command cmd = Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "tag", "--sort=committerdate" })
            .WithValidation(CommandResultValidation.None);

        await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
        {
            if (cmdEvent is StandardOutputCommandEvent stdOut)
            {
                yield return stdOut.Text;
            }
        }
    }

    public async IAsyncEnumerable<string> SearchLogs(string grep, string? regexType = null, string? startIndex = null, string? endIndex = null, string? gitPath = null, string? workingDirectory = null)
    {
        Command cmd = Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(args =>
            {
                args.Add("log");

                if (!string.IsNullOrEmpty(startIndex) && !string.IsNullOrEmpty(endIndex))
                {
                    args.Add($"{startIndex}..{endIndex}");
                }

                if (!string.IsNullOrEmpty(regexType))
                {
                    args.Add($"-{regexType}");
                }

                args.Add($@"--grep=""{grep}""");
            })
            .WithValidation(CommandResultValidation.None);

        await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
        {
            if (cmdEvent is StandardOutputCommandEvent stdOut)
            {
                yield return stdOut.Text;
            }
        }
    }

    public async Task StageFile(string file, string? gitPath = null, string? workingDirectory = null)
    {
        await Cli.Wrap(gitPath ?? "git")
            .WithWorkingDirectory(workingDirectory ?? _fileSystemService.GetCurrentDirectory())
            .WithArguments(new[] { "add", file })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    #endregion Public Methods
}
