using SemanticReleaseCLI.Interfaces;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateTagCommand(IGitService gitService) : AsyncCommand<CreateTagCommand.Settings>
{
    #region Private Fields

    private readonly IGitService _gitService = gitService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await _gitService.CreateAndPushTag(settings.Tag, workingDirectory: settings.RepositoryPath);

        return 0;
    }

    #endregion Public Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
        #region Public Properties

        [Description("Tag to create and push")]
        [CommandArgument(0, "<tag>")]
        public string Tag { get; set; } = "0.0.0.1";

        #endregion Public Properties
    }

    #endregion Public Classes
}
