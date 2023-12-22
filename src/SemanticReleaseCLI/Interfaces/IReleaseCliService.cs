// Ignore Spelling: Cli

namespace SemanticReleaseCLI.Interfaces;

public interface IReleaseCliService
{
    Task Create(string name, string tagName, string commitReference, string description);
}