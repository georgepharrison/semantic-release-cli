using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateChangeLogCommand(IGitService gitService, IRepositoryInformationService repositoryInformationService) : AsyncCommand<CreateChangeLogCommand.Settings>
{
    #region Private Fields

    private readonly IGitService _gitService = gitService;
    private readonly IRepositoryInformationService _repositoryInformationService = repositoryInformationService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        bool isGitRepo = await _gitService.IsGitRepoAsync(settings.RepositoryPath);

        if (!isGitRepo)
        {
            AnsiConsole.WriteLine($"{settings.RepositoryPath ?? "Current directory"} is not a git repository");

            return 1;
        }

        await _repositoryInformationService.CreateChangeLog();

        return 0;
    }

    #endregion Public Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
    }

    #endregion Public Classes
}