using DotLiquid;
using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Interfaces;
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
        => Directory.Delete(RepoPath, true);

    [TestInitialize]
    public void TestInitialize()
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string testDirectory = $"{homeDirectory}/SemanticReleaseTests";

        DirectoryInfo directoryInfo = Directory.CreateDirectory(testDirectory);

        RepoPath = directoryInfo.FullName;

        MockFileSystemService = new(MockBehavior.Strict);
        Mock<IGitService> MockGitService = new(MockBehavior.Strict);

        Subject = new(MockFileSystemService.Object, MockGitService.Object);
    }

    #endregion Public Methods

    #region Public Classes

    [TestClass]
    public class GetChangeLogTemplate : RepositoryServiceTests
    {
        #region Public Methods

        [TestMethod]
        public void When_RepoFileNotFound_Expect_DefaultEmbeddedResource()
        {
            // arrange
            string fileName = Path.Combine(RepoPath, "changelog_template.md");
            string? contents = null;

            MockFileSystemService.Setup(x => x.TryGetFileContents(fileName, out contents)).Returns(false);

            Assembly assembly = typeof(RepositoryServiceTests).Assembly;

            using Stream resourceStream = assembly.GetManifestResourceStream($"{nameof(SemanticReleaseCLI)}.{nameof(UnitTests)}.changelog_template.md")
                ?? throw new KeyNotFoundException("changelog_template.md");

            using StreamReader reader = new(resourceStream, Encoding.UTF8);

            string template = reader.ReadToEnd();

            Template expected = Template.Parse(template);

            // act
            Template actual = Subject.GetChangeLogTemplate(RepoPath);

            // assert
            actual.Should().BeEquivalentTo(expected);
        }

        #endregion Public Methods
    }

    #endregion Public Classes
}
