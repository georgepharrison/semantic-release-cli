using DotLiquid;
using SemanticReleaseCLI.Extensions;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticReleaseCLI;

public sealed partial class RepositoryInformation(
    IGitService gitService,
    IReleaseCliService releaseCliService)
    : IRepositoryInformationService
{
    #region Private Fields

    private static readonly CompositeFormat _changePattern = CompositeFormat.Parse(@"(?<=\n\n\s\s\s\s{0}:\s).*?(?=\n|$)");
    private readonly string _bugFixPattern = string.Format(null, _changePattern, "fix");
    private readonly string _chorePattern = string.Format(null, _changePattern, "chore");
    private readonly string _featurePattern = string.Format(null, _changePattern, "feat");
    private readonly IGitService _gitService = gitService;
    private readonly IReleaseCliService _releaseCliService = releaseCliService;

    #endregion Private Fields

    #region Public Methods

    public async Task CreateChangeLog(string? workingDirectory = null)
    {
        AnsiConsole.MarkupLine("Creating CHANGELOG.md ...");

        List<Release> releases = [];

        List<string> tags = [];

        await foreach (string tag in _gitService.GetTags())
        {
            tags.Add(tag);

            Release release = await CreateRelease(tag, tags);

            releases.Add(release);
        }

        string? commitTag = await _gitService.GetTagAtHead();

        if (commitTag is null)
        {
            Release release = await CreateNewRelease(workingDirectory);

            releases.Add(release);
        }

        CreateFileFromTemplate("CHANGELOG.md", new { releases = releases.Reverse<Release>() });

        AnsiConsole.MarkupLine("CHANGELOG.md created!");
    }

    public async Task CreateRelease(string? workingDirectory = null)
    {
        string? commitTag = await _gitService.GetTagAtHead();

        if (commitTag is not null)
        {
            return;
        }

        await CreateChangeLog(workingDirectory);

        await _gitService.DeleteTags("*-rc");

        commitTag = await GetCommitTag(workingDirectory);

        string version = NonNumericRegex().Replace(commitTag, "");

        await _gitService.CreateAndPushTag(version, workingDirectory: workingDirectory);

        string releaseNotes = await CreateReleaseNotes(workingDirectory);

        string currentCommit = await _gitService.GetCurrentCommit();

        await _releaseCliService.Create($"VERSION {version}", version, currentCommit, releaseNotes);
    }

    public async Task<string> CreateReleaseNotes(string? workingDirectory = null)
    {
        string commitTag = await GetCommitTag(workingDirectory);

        List<Release> releases = [];

        List<string> tags = [];

        await foreach (string tag in _gitService.GetTags())
        {
            tags.Add(tag);
        }

        Release release = await CreateRelease(commitTag, tags);

        releases.Add(release);

        return CreateFileFromTemplate("RELEASENOTES.md", new { releases });
    }

    public async Task<string> GetCommitTag(string? workingDirectory = null)
    {
        string? tag = await _gitService.GetTagAtHead(workingDirectory: workingDirectory);

        if (!string.IsNullOrEmpty(tag))
        {
            return tag;
        }

        string? commitPreviousTag = await GetCommitPreviousTag(workingDirectory: workingDirectory);

        int majorVersion = string.IsNullOrEmpty(commitPreviousTag) ? 0 : Convert.ToInt32(commitPreviousTag.Split('.')[0]);

        List<string> tags = [];

        await foreach (string item in _gitService.GetTags(workingDirectory: workingDirectory))
        {
            tags.Add(item);
        }

        if (tags.Count is 0)
        {
            List<string> items = [];

            await foreach (string item in _gitService.SearchLogs(@"BUMP\sVERSION", workingDirectory: workingDirectory))
            {
                items.Add(item);
            }

            if (items.Count > 0)
            {
                majorVersion = 1;
            }
        }
        else
        {
            List<string> items = [];

            string startIndex = string.IsNullOrEmpty(commitPreviousTag) ? "0" : commitPreviousTag;
            string endIndex = "HEAD";

            await foreach (string item in _gitService.SearchLogs(@"BUMP\sVERSION", regexType: "P", startIndex: startIndex, endIndex: endIndex, workingDirectory: workingDirectory))
            {
                items.Add(item);
            }

            if (items.Count > 0)
            {
                majorVersion++;
            }
        }

        string latestCommitDate = await _gitService.GetCommitDate(workingDirectory: workingDirectory);

        int commits = await _gitService.GetCommitCount($"{latestCommitDate}T00:00:00", $"{latestCommitDate}T23:59:59", workingDirectory: workingDirectory);

        string year = latestCommitDate.Substring(2, 2);
        string month = latestCommitDate.Substring(5, 2).TrimStart('0');
        string day = latestCommitDate.Substring(8, 2);

        return $"{majorVersion}.{year}.{month}{day}.{commits}";
    }

    #endregion Public Methods

    #region Private Methods

    private static string CreateFileFromTemplate(string file, object anonymousObject)
    {
        using StreamWriter w = new(file);

        Template t = GetTemplate("CHANGELOG.md");

        string rendering = t.Render(Hash.FromAnonymousObject(anonymousObject));

        w.Write(rendering);

        AnsiConsole.MarkupLine("RELEASENOTES.md created!");

        return rendering;
    }

    private static Template GetTemplate(string templateName, Assembly? assembly = null)
    {
        assembly ??= typeof(RepositoryInformation).Assembly;

        using Stream resourceStream = assembly.GetManifestResourceStream($"SemanticReleaseCLI.Templates.{templateName}")
            ?? throw new KeyNotFoundException($"SemanticReleaseCLI.Templates.{templateName}");

        using StreamReader reader = new(resourceStream, Encoding.UTF8);

        string template = reader.ReadToEnd();

        return Template.Parse(template);
    }

    [GeneratedRegex("[^.0-9]")]
    private static partial Regex NonNumericRegex();

    private async Task<Release> CreateNewRelease(string? workingDirectory = null)
    {
        string releaseDate = await _gitService.GetCommitDate();

        string? commitPreviousTag = await GetCommitPreviousTag();

        List<string> changes = [];

        await foreach (string item in _gitService.GetLogs(startIndex: commitPreviousTag))
        {
            changes.Add(item);
        }

        string commitHash = await GetCommitTag(workingDirectory: workingDirectory);

        return CreateRelease(commitHash, releaseDate, changes);
    }

    private async Task<Release> CreateRelease(string tag, List<string> tags)
    {
        string releaseDate = await _gitService.GetCommitDate(reference: tag);

        List<string> changes = [];

        int tagIndex = tags.IndexOf(tag);

        string? startIndex = tagIndex is 0 ? null : tags[tagIndex - 1];

        await foreach (string change in _gitService.GetLogs(startIndex: startIndex, endIndex: tag))
        {
            changes.Add(change);
        }

        return CreateRelease(tag, releaseDate, changes);
    }

    private Release CreateRelease(string name, string releaseDate, IEnumerable<string> changes)
    {
        List<string> features = [];
        List<string> bugFixes = [];
        List<string> chores = [];

        foreach (string message in changes)
        {
            Match feature = Regex.Match(message, _featurePattern);

            if (feature.Success)
            {
                features.Add(feature.Value.FirstCharToUpper());
                continue;
            }

            Match bugFix = Regex.Match(message, _bugFixPattern);

            if (bugFix.Success)
            {
                bugFixes.Add(bugFix.Value.FirstCharToUpper());
                continue;
            }

            Match chore = Regex.Match(message, _chorePattern);

            if (chore.Success)
            {
                chores.Add(chore.Value.FirstCharToUpper());
                continue;
            }
        }

        return new(name, releaseDate, features, bugFixes, chores);
    }

    private async Task<string?> GetCommitPreviousTag(string? workingDirectory = null)
        => await _gitService.GetTagBeforeHead(workingDirectory: workingDirectory);

    #endregion Private Methods
}
