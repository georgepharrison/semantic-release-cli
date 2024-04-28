using SemanticReleaseCLI.Commands;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI;

internal abstract class AbstractAsyncCommand<TSettings>(IFileSystemService fileSystemService, IGitService gitService) : AsyncCommand<TSettings>
    where TSettings : AppSettings
{
    #region Private Fields

    private readonly IGitService _gitService = gitService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        settings.RepositoryPath ??= FileSystemService.GetCurrentDirectory();

        bool isGitRepo = await _gitService.IsGitRepoAsync(settings.RepositoryPath);

        if (!isGitRepo)
        {
            AnsiConsole.WriteLine($"{settings.RepositoryPath ?? "Current directory"} is not a git repository");

            return 1;
        }

        return 0;
    }

    #endregion Public Methods

    #region Protected Properties

    protected IFileSystemService FileSystemService { get; } = fileSystemService;

    #endregion Protected Properties
}
