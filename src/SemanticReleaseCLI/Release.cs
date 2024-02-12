namespace SemanticReleaseCLI;

public sealed class Release
{
    #region Private Members

    private readonly IReadOnlyList<ConventionalCommit> _commits;
    private readonly IReadOnlyList<GitCommit> _gitCommits;

    #endregion Private Members
    
    #region Public Constructors
    
    public Release(string name, IEnumerable<GitCommit> commits, string? parentName = null)
    {
        _gitCommits = commits.OrderByDescending(x => x.AuthorDate).ToList();

        Date = DateOnly.FromDateTime(_gitCommits[0].AuthorDate);

        _commits = _gitCommits
            .Where(ConventionalCommit.IsConventionalCommit)
            .Select(x => new ConventionalCommit(x))
            .ToList()
            .AsReadOnly();

        Name = GetName(name, parentName);
    }

    #endregion Public Constructors

    #region Public Methods
    
    public IReadOnlyList<ConventionalCommit> GetChangesByType(string type)
        => _commits.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

    private string GetName(string name, string? parentName)
    {
        if (name != "HEAD")
        {
            return name;
        }

        DateTime authorDate = _commits[0].GitCommit.AuthorDate;

        int bumps = _commits.Sum(x => x.VersionBumps);

        int major = int.Parse(parentName?.Split('.').FirstOrDefault() ?? "0") + bumps;

        string year = authorDate.Year.ToString().Substring(2, 2);

        string month = authorDate.Month.ToString().TrimStart('0');

        string day = authorDate.Day.ToString();

        int commitNumberOfDay = _commits.Count(x => x.GitCommit.AuthorDate.Date == authorDate.Date);

        return $"{major}.{year}.{month}{day}.{commitNumberOfDay}";
    }

    #endregion Public Methods

    #region Public Properties

    public DateOnly Date { get; }

    public string CurrentCommitId
        => _gitCommits[0].Id;

    public string Name { get; }
 
    #endregion Public Properties
}
