using CliWrap;
using CliWrap.EventStream;
using SemanticReleaseCLI.Interfaces;
using Spectre.Console;

namespace SemanticReleaseCLI.Services;

public class ReleaseCliService : IReleaseCliService
{
    #region Public Methods

    public async Task Create(string name, string tagName, string commitReference, string description)
    {
        Command cmd = Cli.Wrap("release-cli")
            .WithWorkingDirectory(Directory.GetCurrentDirectory())
            .WithArguments(args =>
            {
                args.Add("create");
                args.Add("--name");
                args.Add(name);
                args.Add("--tag-name");
                args.Add(tagName);
                args.Add("--description");
                args.Add(description);
                args.Add("--ref");
                args.Add(commitReference);
            })
            .WithValidation(CommandResultValidation.None);

        await foreach (CommandEvent cmdEvent in cmd.ListenAsync())
        {
            switch (cmdEvent)
            {
                case StartedCommandEvent started:
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
