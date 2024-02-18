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
            Release release = await GetReleaseAsync(settings.RepositoryPath!);

            string releaseNotes = CreateReleaseNotes(settings.RepositoryPath!, release);

            await CreateVersionEnvironmentVariableFileAsync(release);

            if (settings.IsDryRun)
            {
                AnsiConsole.Write(releaseNotes);
            }
            else
            {
                await _releaseCliService.CreateAsync($"VERSION {release.Name}", release.Name, release.CurrentCommitId, releaseNotes);
            }

            returnCode = 0;
        }

        return returnCode;
    }

    #endregion Public Methods

    #region Private Methods

    private string CreateReleaseNotes(string repoPath, Release release)
    {
        IReadOnlyList<dynamic> templateData = _repositoryService.GetTemplateData(repoPath, release);

        Template template = _repositoryService.GetChangeLogTemplate(repoPath);

        return template.Render(Hash.FromAnonymousObject(new { releases = templateData }));
    }

    private async Task CreateVersionEnvironmentVariableFileAsync(Release release)
    {
        string versionEnvironmentVariable = $"VERSION={release.Name}";

        await FileSystemService.WriteAllTextAsync("version.env", versionEnvironmentVariable);
    }

    private async Task<Release> GetReleaseAsync(string repoPath)
        => (await _repositoryService.GetReleasesAsync(repoPath))[0];

    #endregion Private Methods

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
