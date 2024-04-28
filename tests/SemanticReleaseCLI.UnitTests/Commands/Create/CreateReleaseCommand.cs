using Moq;
using SemanticReleaseCLI.Commands.Create;
using SemanticReleaseCLI.Interfaces;
using SemanticReleaseCLI.Services;
using Spectre.Console.Cli;
using System.Reflection;
using System.Text;

namespace SemanticReleaseCLI.UnitTests.Commands.Create;

[TestClass]
public class CreateReleaseCommandTests
{
    #region Protected Properties

    protected Mock<IFileSystemService> MockFileSystemService { get; private set; } = default!;
    protected Mock<IReleaseCliService> MockReleaseCliService { get; private set; } = default!;
    protected string RepoPath { get; private set; } = string.Empty;
    private CreateReleaseCommand Subject { get; set; } = default!;
    private GitService GitService { get; set; } = default!;
    private RepositoryService RepositoryService { get; set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    [TestCleanup]
    public void TestCleanup()
    {
        DirectoryInfo directory = new(RepoPath);

        if (!directory.Exists)
        {
            return;
        }

        RemoveDirectory(directory);

        directory.Delete();

        static void RemoveDirectory(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                RemoveDirectory(dir);

                dir.Delete();
            }
        }
    }

    [TestInitialize]
    public void TestInitialize()
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string testDirectory = $"{homeDirectory}/SemanticReleaseTests";

        DirectoryInfo directoryInfo = Directory.CreateDirectory(testDirectory);

        RepoPath = directoryInfo.FullName;

        MockFileSystemService = new(MockBehavior.Strict);

        GitService = new(MockFileSystemService.Object);

        MockReleaseCliService = new(MockBehavior.Strict);

        RepositoryService = new(MockFileSystemService.Object, GitService);

        Subject = new(MockFileSystemService.Object, GitService, MockReleaseCliService.Object, RepositoryService);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_Work()
    {
        GitServiceTests gitServiceTests = new();

        gitServiceTests.TestInitialize();

        await gitServiceTests.SeuptGitRepoAsync();

        Mock<IRemainingArguments> mockRemainingArguments = new(MockBehavior.Loose);

        CommandContext context = new(mockRemainingArguments.Object, "Foo", null);

        CreateReleaseCommand.Settings settings = new();

        MockFileSystemService.Setup(x => x.GetCurrentDirectory()).Returns(RepoPath);

        string? changeTypes = GetChangeLogTypesContents();

        MockFileSystemService.Setup(x => x.TryGetFileContents($@"{RepoPath}\changelog_types.json", out changeTypes)).Returns(true);

        string? changeLogTemplate = GetTemplateContents();

        MockFileSystemService.Setup(x => x.TryGetFileContents($@"{RepoPath}\changelog_template.md", out changeLogTemplate)).Returns(true);

        int exitCode = await Subject.ExecuteAsync(context, settings);
    }

    #endregion Public Methods

    #region Helper Methods

    private string GetTemplateContents()
    {
        Assembly assembly = typeof(RepositoryServiceTests).Assembly;

        using Stream resourceStream = assembly.GetManifestResourceStream($"{nameof(SemanticReleaseCLI)}.{nameof(UnitTests)}.changelog_template.md")
            ?? throw new KeyNotFoundException("changelog_template.md");

        using StreamReader reader = new(resourceStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    private string GetChangeLogTypesContents()
    {
        Assembly assembly = typeof(RepositoryServiceTests).Assembly;

        using Stream resourceStream = assembly.GetManifestResourceStream($"{nameof(SemanticReleaseCLI)}.{nameof(UnitTests)}.changelog_types.json")
            ?? throw new KeyNotFoundException("changelog_types.json");

        using StreamReader reader = new(resourceStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    #endregion Helper Methods
}
