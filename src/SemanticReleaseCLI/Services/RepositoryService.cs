
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotLiquid;
using SemanticReleaseCLI.Interfaces;

namespace SemanticReleaseCLI;

public class RepositoryService(
    IFileSystemService fileSystemService,
    IGitService gitService) : IRepositoryService
{
    #region Private Members

    private const string _changeLogTemplateFileName = "changelog_template.md";
    private const string _changeLogTypesFileName = "changelog_types.json";
    private const string _defaultChangeLogTemplateFile = $"{nameof(SemanticReleaseCLI)}.{_changeLogTemplateFileName}";
    private const string _defaultChangeLogTypesFile = $"{nameof(SemanticReleaseCLI)}.{_changeLogTypesFileName}";
    private readonly IFileSystemService _fileSystemService = fileSystemService;
    private readonly IGitService _gitService = gitService;

    #endregion Private Members

    #region Public Methods

    public Template GetChangeLogTemplate(string repoPath)
    {
        string fileName = Path.Combine(repoPath, _changeLogTemplateFileName);

        if (!_fileSystemService.TryGetFileContents(fileName, out string? contents))
        {
            Assembly assembly = typeof(RepositoryService).Assembly;

            using Stream resourceStream = assembly.GetManifestResourceStream(_defaultChangeLogTemplateFile)
                ?? throw new KeyNotFoundException(_defaultChangeLogTemplateFile);

            using StreamReader reader = new(resourceStream, Encoding.UTF8);

            contents = reader.ReadToEnd();
        }

        // TODO: Error Handling
        // Need to ensure that the user's values are correct and doesn't throw an exception
        return Template.Parse(contents);
    }

    public async Task<IReadOnlyList<Release>> GetReleasesAsync(string repoPath)
    {
        IReadOnlyList<GitCommit> commits = await _gitService.GetAllCommitsAsync(repoPath: repoPath);

        List<Release> releases = [];
        List<GitCommit> releaseCommits = [];
        string tag = "HEAD";

        foreach (GitCommit commit in commits)
        {
            var match = Regex.Match(commit.RefNames, @"(?<=tag: )[0-9.]*(?=,|\))");

            if (match.Success)
            {
                if (releaseCommits.Count > 0)
                {
                    releases.Add(new(tag, releaseCommits, match.Value));
                    releaseCommits.Clear();
                }

                tag = match.Value;
            }

            releaseCommits.Add(commit);
        }

        releases.Add(new(tag, releaseCommits));

        return releases.AsReadOnly();
    }

    public IReadOnlyList<dynamic> GetTemplateData(string repoPath, Release release)
    {
        List<ChangeTypeOptions> changeTypeOptions = GetChangeTypeOptions(repoPath);

        List<dynamic> templatedReleases = [];

        dynamic templatedRelease = GetTemplatedRelease(release, changeTypeOptions);

        templatedReleases.Add(templatedRelease);

        return templatedReleases.AsReadOnly();
    }

    public IReadOnlyList<dynamic> GetTemplateData(string repoPath, IEnumerable<Release> releases)
    {
        List<ChangeTypeOptions> changeTypeOptions = GetChangeTypeOptions(repoPath);

        List<dynamic> templatedReleases = [];

        foreach (Release release in releases)
        {
            dynamic templatedRelease = GetTemplatedRelease(release, changeTypeOptions);

            templatedReleases.Add(templatedRelease);
        }

        return templatedReleases.AsReadOnly();
    }

    #endregion Public Methods

    #region Private Methods

    private static List<dynamic> GetChangeTypes(Release release, List<ChangeTypeOptions> changeTypeOptions)
    {
        List<dynamic> changeTypes = [];

        foreach (ChangeTypeOptions options in changeTypeOptions)
        {
            IReadOnlyList<ConventionalCommit> changes = release.GetChangesByType(options.Type);

            List<string> changesWithoutScope = changes
                .Where(x => x.Scope is null)
                .Select(x => x.Description)
                .ToList();

            var scopes = changes
                .Where(x => x.Scope is not null)
                .GroupBy(x => x.Scope)
                .Select(x => new { name = x.Key, changes = x.Select(y => y.Description) });

            dynamic changeType = new ExpandoObject();

            ((IDictionary<string, object>)changeType).Add("heading", options.Heading);
            ((IDictionary<string, object>)changeType).Add("changes_without_scope", changesWithoutScope);
            ((IDictionary<string, object>)changeType).Add("scopes", scopes);

            changeTypes.Add(changeType);
        }

        return changeTypes;
    }

    private static dynamic GetTemplatedRelease(Release release, List<ChangeTypeOptions> changeTypeOptions)
    {
        dynamic templatedRelease = new ExpandoObject();

        templatedRelease.name = release.Name;
        templatedRelease.date = release.Date.ToString();

        List<dynamic> changeTypes = GetChangeTypes(release, changeTypeOptions);

        ((IDictionary<string, object>)templatedRelease).Add("change_types", changeTypes);

        return templatedRelease;
    }

    private List<ChangeTypeOptions> GetChangeTypeOptions(string repoPath)
    {
        string fileName = Path.Combine(repoPath, _changeLogTypesFileName);

        if (!_fileSystemService.TryGetFileContents(fileName, out string? contents))
        {
            Assembly assembly = typeof(RepositoryService).Assembly;

            using Stream resourceStream = assembly.GetManifestResourceStream(_defaultChangeLogTypesFile)
                ?? throw new KeyNotFoundException(_defaultChangeLogTypesFile);

            using StreamReader reader = new(resourceStream, Encoding.UTF8);

            contents = reader.ReadToEnd();
        }

        // TODO: Error Handling
        // Need to ensure that the user's values are correct and doesn't throw an exception
        return JsonSerializer.Deserialize<List<ChangeTypeOptions>>(contents) ?? [];
    }

    #endregion Private Methods

    #region Public Classes

    private sealed class ChangeTypeOptions
    {
        public string Heading { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    #endregion Public Classes
}
