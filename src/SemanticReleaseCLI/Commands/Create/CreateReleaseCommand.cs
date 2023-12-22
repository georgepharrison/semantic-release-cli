using SemanticReleaseCLI.Interfaces;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateReleaseCommand(IRepositoryInformationService repositoryInformationService) : AsyncCommand<CreateReleaseCommand.Settings>
{
    #region Private Fields

    private readonly IRepositoryInformationService _repositoryInformationService = repositoryInformationService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await _repositoryInformationService.CreateRelease();

        return 0;
    }

    #endregion Public Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
    }

    #endregion Public Classes
}