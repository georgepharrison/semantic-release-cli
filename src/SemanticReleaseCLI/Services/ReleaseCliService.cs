using CliWrap;
using CliWrap.EventStream;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;

namespace SemanticReleaseCLI.Services;

public sealed class ReleaseCliService : IReleaseCliService
{
    #region Public Methods

    public async Task CreateAsync(string name, string tagName, string commitReference, string description)
    {
        Command cmd = Cli.Wrap("release-cli")
            .WithWorkingDirectory(Directory.GetCurrentDirectory())
            .WithArguments(args => args
                .Add("create")
                .Add("--name")
                .Add(name)
                .Add("--tag-name")
                .Add(tagName)
                .Add("--description")
                .Add(description)
                .Add("--ref")
                .Add(commitReference)
            )
            .WithValidation(CommandResultValidation.None);

        await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
        {
            switch (cmdEvent)
            {
                case StartedCommandEvent:
                    AnsiConsole.WriteLine("Creating GitLab Release");
                    break;

                case StandardOutputCommandEvent stdOut:
                    AnsiConsole.WriteLine(stdOut.Text);
                    break;

                case StandardErrorCommandEvent stdErr:
                    AnsiConsole.WriteLine(stdErr.Text);
                    break;

                default:
                    break;
            }
        }
    }

    #endregion Public Methods
}
