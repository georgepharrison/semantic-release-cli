using Spectre.Console.Cli;
using System.ComponentModel;

namespace SemanticReleaseCLI.Commands;

internal class AppSettings : CommandSettings
{
    #region Public Properties

    [Description("The path to your git repository")]
    [CommandOption("-p|--path")]
    public string? RepositoryPath { get; init; }

    #endregion Public Properties
}