using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.RegularExpressions;

namespace SemanticReleaseCLI.Commands;

internal sealed partial class GenerateVersionCommand(IRepositoryInformationService repositoryInformationService) : AsyncCommand<GenerateVersionCommand.Settings>
{
    #region Private Fields

    private readonly IRepositoryInformationService _repositoryInformationService = repositoryInformationService;

    [GeneratedRegex("[^.0-9]")]
    private static partial Regex NonNumericRegex();

    #endregion Private Fields

    #region Public Methods

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        string result = await _repositoryInformationService.GetCommitTag();

        string version = NonNumericRegex().Replace(result, "");

        string file = Path.Combine(settings.RepositoryPath, "version.env");

        using StreamWriter writer = new(file);

        string content = $"VERSION={version}";

        await writer.WriteAsync(content);

        AnsiConsole.MarkupLine($"{file} created");
        AnsiConsole.MarkupLine(content);

        return 0;
    }

    #endregion Public Methods

    #region Public Classes

    public sealed class Settings : AppSettings
    {
    }

    #endregion Public Classes
}