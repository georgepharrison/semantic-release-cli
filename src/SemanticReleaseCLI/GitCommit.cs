namespace SemanticReleaseCLI;

public sealed class GitCommit
{
    #region Public Properties

    public DateTime AuthorDate { get; init; }
    
    public string AuthorEmail { get; init; } = default!;
    
    public string AuthorName { get; init; } = default!;
    
    public string Body { get; init; } = default!;
    
    public string Id { get; init; } = default!;
    
    public string? ParentId { get; init; }
    
    public string RefNames { get; init; } = default!;

    public string Subject { get; init; } = default!;
    
    #endregion Public Properties
}
