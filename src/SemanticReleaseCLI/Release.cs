using DotLiquid;
using SemanticReleaseCLI.Extensions;

internal sealed class Release(string name, string date, IEnumerable<string> features, IEnumerable<string> bugFixes, IEnumerable<string> chores) : Drop
{
    #region Public Properties

    public IReadOnlyList<string> BugFixes { get; } = bugFixes.ToReadOnlyList();
    public IReadOnlyList<string> Chores { get; } = chores.ToReadOnlyList();
    public string Date { get; } = date;
    public IReadOnlyList<string> Features { get; } = features.ToReadOnlyList();
    public string Name { get; } = name;

    #endregion Public Properties
}