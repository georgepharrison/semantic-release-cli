using DotLiquid;
using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Services;
using System.Reflection;
using System.Text;

namespace SemanticReleaseCLI.UnitTests;

public class RepositoryServiceTests
{
    #region Protected Properties

    protected Mock<IFileSystemService> MockFileSystemService { get; private set; } = default!;
    protected string RepoPath { get; private set; } = string.Empty;
    protected RepositoryService Subject { get; private set; } = default!;

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

        GitService gitService = new(MockFileSystemService.Object);

        Subject = new(MockFileSystemService.Object, gitService);
    }

    #endregion Public Methods

    #region Public Classes

    [TestClass]
    public class GetChangeLogTemplate : RepositoryServiceTests
    {
        #region Helper Methods

        private string GetTemplateContents()
        {
            Assembly assembly = typeof(RepositoryServiceTests).Assembly;

            using Stream resourceStream = assembly.GetManifestResourceStream($"{nameof(SemanticReleaseCLI)}.{nameof(UnitTests)}.changelog_template.md")
                ?? throw new KeyNotFoundException("changelog_template.md");

            using StreamReader reader = new(resourceStream, Encoding.UTF8);

            return reader.ReadToEnd();
        }

        #endregion Helper Methods

        #region Public Methods

        [TestMethod]
        public void When_RepoFileNotFound_Expect_DefaultEmbeddedResource()
        {
            // arrange
            string fileName = Path.Combine(RepoPath, "changelog_template.md");
            string? contents = null;

            MockFileSystemService.Setup(x => x.TryGetFileContents(fileName, out contents)).Returns(false);

            string template = GetTemplateContents();

            Template expected = Template.Parse(template);

            // act
            Template actual = Subject.GetChangeLogTemplate(RepoPath);

            // assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void When_RepoFileFound_Expect_Template()
        {
            // arrange
            string fileName = Path.Combine(RepoPath, "changelog_template.md");
            string? contents = GetTemplateContents();

            MockFileSystemService.Setup(x => x.TryGetFileContents(fileName, out contents)).Returns(true);

            Template expected = Template.Parse(contents);

            // act
            Template actual = Subject.GetChangeLogTemplate(RepoPath);

            // assert
            actual.Should().BeEquivalentTo(expected);
        }

        #endregion Public Methods
    }

    [TestClass]
    public class GetReleasesAsync : RepositoryServiceTests
    {
        [TestMethod]
        public async Task When_True_Expect_True()
        {
            GitServiceTests gitServiceTests = new();

            gitServiceTests.TestInitialize();

            await gitServiceTests.SeuptGitRepoAsync();

            IReadOnlyList<Release> actual = await Subject.GetReleasesAsync(RepoPath);
        }
    }

    #endregion Public Classes
}
