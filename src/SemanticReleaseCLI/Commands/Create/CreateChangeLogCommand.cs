using SemanticReleaseCLI.Interfaces;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateChangeLogCommand(IRepositoryInformationService repositoryInformationService) : AsyncCommand<CreateChangeLogCommand.Settings>
{
    #region Private Fields

    private readonly IRepositoryInformationService _repositoryInformationService = repositoryInformationService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
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