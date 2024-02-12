using System.Text;
using System.Text.RegularExpressions;
using SemanticReleaseCLI.Extensions;

namespace SemanticReleaseCLI;

public sealed partial class ConventionalCommit
{
    #region Regex Partials

    [GeneratedRegex($@"^(?<{_typeGroupName}>[a-z]*)(\((?<{_scopeGroupName}>[a-z]*)\))?(:\s)(?<{_descriptionGroupName}>.*)", RegexOptions.None, matchTimeoutMilliseconds: 1_000)]
    private static partial Regex ConventionalCommitRegex();

    [GeneratedRegex($@"((?<{_keyGroupName}>^([\w-]*|BREAKING CHANGE))(?::\s|\s#)(?<{_valueGroupName}>.*$))|^BUMP VERSION$", RegexOptions.None, matchTimeoutMilliseconds: 1_000)]
    private static partial Regex FooterRegex();

    #endregion Regex Partials

    #region Private Members

    private const string _descriptionGroupName = "description";
    private const string _keyGroupName = "key";
    private const string _scopeGroupName = "scope";
    private const string _typeGroupName = "type";
    private const string _valueGroupName = "value";
    private readonly List<string> _footers = [];

    #endregion Private Members
    
    #region Public Constructors
    
    public ConventionalCommit(GitCommit gitCommit)
    {
        GitCommit = gitCommit;

        Match match = ConventionalCommitRegex().Match(GitCommit.Subject);

        Type = match.Groups[_typeGroupName].Value;
        
        Description = match.Groups[_descriptionGroupName].Value.FirstCharToUpper();
        
        if (match.Groups[_scopeGroupName].Length > 0)
        {
            Scope = match.Groups[_scopeGroupName].Value;
        }
        
        Body = GetBodyAndSetFooters();
    }

    #endregion Public Constructors

    #region Public Methods

    public static bool IsConventionalCommit(GitCommit commit)
        => ConventionalCommitRegex().IsMatch(commit.Subject);

    public string? GetBodyAndSetFooters()
    {
        StringBuilder stringBuilder = new();

        foreach (string line in GitCommit.Body.Split(Environment.NewLine))
        {
            Match match = FooterRegex().Match(line);

            if (match.Success)
            {
                _footers.Add(line);
            }
            else
            {
                stringBuilder.AppendLine(line);
            }
        }

        string body = stringBuilder.ToString().Trim();

        return string.IsNullOrEmpty(body) ? null : body;
    }

    #endregion Public Methods

    #region Public Properties

    public string? Body { get; }

    public string Description { get; }

    public IReadOnlyList<string> Footers
        => _footers.AsReadOnly();

    public GitCommit GitCommit { get; }

    public string? Scope { get; }

    public string Type { get; }

    public int VersionBumps
        => _footers.Count(footer =>
        {
            return footer.Contains("BREAKING CHANGE", StringComparison.Ordinal)
                || footer.Contains("BUMP VERSION", StringComparison.Ordinal);
        });

    #endregion Public Properties
}
