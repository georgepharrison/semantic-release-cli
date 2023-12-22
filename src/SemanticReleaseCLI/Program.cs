using Microsoft.Extensions.DependencyInjection;
using SemanticReleaseCLI.Commands;
using SemanticReleaseCLI.Commands.Create;
using SemanticReleaseCLI.Interfaces;
using SemanticReleaseCLI.Services;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI;

internal sealed class Program
{
    #region Private Methods

    private static int Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();

        services.AddTransient<IGitService, GitService>();
        services.AddTransient<IReleaseCliService, ReleaseCliService>();
        services.AddTransient<IRepositoryInformationService, RepositoryInformation>();

        TypeRegistrar registrar = new(services);

        CommandApp app = new(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("semantic-release-cli");

            config.AddBranch<AppSettings>("create", create =>
            {
                create.AddCommand<CreateChangeLogCommand>("changelog")
                    .WithDescription("Create a CHANGELOG.md file")
                    .WithExample(["create", "changelog"]);

                create.AddCommand<CreateReleaseCommand>("release")
                    .WithDescription("Create a release")
                    .WithExample(["create", "release"]);

                create.AddCommand<CreateTagCommand>("tag")
                    .WithDescription("Create a tag")
                    .WithExample(["create", "tag"]);
            });

            config.AddCommand<GenerateVersionCommand>("generate")
                .WithDescription("Create a semantic version for the latest commit and export it to version.env.")
                .WithExample(["generate"]);
        });

        return app.Run(args);
    }

    #endregion Private Methods
}
