using DotLiquid;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

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

        if (!settings.IsDryRun && (string.IsNullOrEmpty(settings.Author) || string.IsNullOrEmpty(settings.Branch)))
        {
            throw new InvalidOperationException("Must provide both author and branch command options when not performing a dry run");
        }

        if (returnCode is 0)
        {
            IReadOnlyList<Release> releases = await _repositoryService.GetReleasesAsync(settings.RepositoryPath!);

            string changeLog = CreateChangeLog(settings.RepositoryPath!, releases);

            if (settings.IsDryRun)
            {
                AnsiConsole.Write(changeLog);
            }
            else
            {
                string version = releases[0].Name;

                AnsiConsole.WriteLine($"VERSION={version}");

                string fileName = await FileSystemService.WriteAllTextAsync("CHANGELOG.md", changeLog, settings.RepositoryPath!, settings.OutputDirectory);

                await _repositoryService.AddChangeLogAsync(fileName, version, settings.Author, settings.Branch, settings.RepositoryPath!);
            }

            returnCode = 0;
        }

        return returnCode;
    }

    #endregion Public Methods

    #region Private Methods

    private string CreateChangeLog(string repoPath, IEnumerable<Release> releases)
    {
        IReadOnlyList<dynamic> templateData = _repositoryService.GetTemplateData(repoPath, releases);

        Template template = _repositoryService.GetChangeLogTemplate(repoPath);

        return template.Render(Hash.FromAnonymousObject(new { releases = templateData }));
    }

    #endregion Private Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
        #region Public Properties

        [Description("Author of change log git commit")]
        [CommandOption("-a")]
        public string Author { get; set; } = string.Empty;

        [Description("Branch for change log git commit")]
        [CommandOption("-b")]
        public string Branch { get; set; } = string.Empty;

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
