using Microsoft.Extensions.DependencyInjection;
using SemanticReleaseCLI.Commands;
using SemanticReleaseCLI.Commands.Create;
using SemanticReleaseCLI.Interfaces;
using SemanticReleaseCLI.Services;
using Spectre.Console.Cli;

namespace SemanticReleaseCLI;

static class Program
{
    #region Private Methods

    private static int Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();

        services.AddTransient<IFileSystemService, FileSystemService>();
        services.AddTransient<IGitService, GitService>();
        services.AddTransient<IReleaseCliService, ReleaseCliService>();
        services.AddTransient<IRepositoryService, RepositoryService>();

        TypeRegistrar registrar = new(services);

        CommandApp app = new(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("semantic-release-cli");

            config.AddBranch<AppSettings>("create", create =>
            {
                create.AddCommand<CreateChangeLogCommand>("changelog")
                    .WithDescription("Create a CHANGELOG.md file")
                    .WithExample("create", "changelog");

                create.AddCommand<CreateReleaseCommand>("release")
                    .WithDescription("Create a release")
                    .WithExample("create", "release");
            });
        });

        return app.Run(args);
    }

    #endregion Private Methods
}
