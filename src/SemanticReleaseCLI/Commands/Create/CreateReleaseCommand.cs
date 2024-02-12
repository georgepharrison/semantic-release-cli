using DotLiquid;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateReleaseCommand(
    IFileSystemService fileSystemService,
    IGitService gitService,
    IReleaseCliService releaseCliService,
    IRepositoryService repositoryService)
    : AbstractAsyncCommand<CreateReleaseCommand.Settings>(fileSystemService, gitService)
{
    #region Private Fields

    private readonly IReleaseCliService _releaseCliService = releaseCliService;
    private readonly IRepositoryService _repositoryService = repositoryService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        int returnCode = await base.ExecuteAsync(context, settings);

        if (returnCode is 0)
        {
            IReadOnlyList<Release> releases = await _repositoryService.GetReleasesAsync(settings.RepositoryPath!);

            Release release = releases[0];

            IReadOnlyList<dynamic> templateData = _repositoryService.GetTemplateData(settings.RepositoryPath!, release);

            Template template = _repositoryService.GetChangeLogTemplate(settings.RepositoryPath!);

            string releaseNotes = template.Render(Hash.FromAnonymousObject(new { releases = templateData }));

            if (settings.IsDryRun)
            {
                // TODO: need to return version file as well for pipelines to use
                AnsiConsole.Write(releaseNotes);
            }
            else
            {
                //await _releaseCliService.Create($"VERSION {release.Name}", release.Name, release.CurrentCommitId, releaseNotes);
            }

            returnCode = 0;
        }

        return returnCode;
    }

    #endregion Public Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
        #region Public Properties

        [Description("Perform a dry run")]
        [CommandOption("--dry-run")]
        public bool IsDryRun { get; set; } = false;

        #endregion Public Properties
    }

    #endregion Public Classes
}
