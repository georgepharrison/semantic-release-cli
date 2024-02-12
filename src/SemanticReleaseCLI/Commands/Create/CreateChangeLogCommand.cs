using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DotLiquid;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI.Commands.Create;

internal sealed class CreateChangeLogCommand(
    IFileSystemService fileSystemService,
    IGitService gitService,
    IRepositoryService repositoryService)
    : AbstractAsyncCommand<CreateChangeLogCommand.Settings>(fileSystemService, gitService)
{
    #region Private Fields

    private readonly IRepositoryService _repositoryService = repositoryService;

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        int returnCode = await base.ExecuteAsync(context, settings);
        
        if (returnCode is 0)
        {
            IReadOnlyList<Release> releases = await _repositoryService.GetReleasesAsync(settings.RepositoryPath!);

            string releaseVersion = releases[0].Name;

            AnsiConsole.WriteLine($"RELEASE_VERSION={releaseVersion}");

            IReadOnlyList<dynamic> templateData = _repositoryService.GetTemplateData(settings.RepositoryPath!, releases);

            Template template = _repositoryService.GetChangeLogTemplate(settings.RepositoryPath!);

            string changeLog = template.Render(Hash.FromAnonymousObject(new { releases = templateData }));

            if (settings.IsDryRun)
            {
                AnsiConsole.Write(changeLog);
            }
            else
            {
                string changelogDirectory = Path.Combine(settings.RepositoryPath!, settings.OutputDirectory);

                Directory.CreateDirectory(changelogDirectory);

                string fileName = Path.Combine(changelogDirectory, "CHANGELOG.md");

                await File.WriteAllTextAsync(fileName, changeLog);

                // TODO: add CHANGELOG.md and commit
                // git add CHANGELOG.md
                // commit commit -m "chore(CHANGELOG): 0.00.0000.0" --author=LAST_COMMIT_AUTHOR
                // git push origin HEAD:branch
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

        [Description("Directory to create CHANGELOG.md relative to git repo root (default is ./docs)")]
        [CommandOption("-o|--output")]
        public string OutputDirectory { get; set; } = "docs/";

        #endregion Public Properties
    }

    #endregion Public Classes
}