using DotLiquid;
using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Interfaces;

namespace SemanticReleaseCLI.UnitTests;

public class RepositoryServiceTests
{
    #region Protected Properties

    protected Mock<IFileSystemService> MockFileSystemService { get; private set; } = default!;
    protected string RepoPath { get; private set; } = string.Empty;
    protected RepositoryService Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

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

    [TestCleanup]
    public void TestCleanup()
        => Directory.Delete(RepoPath, true);

    #endregion Public Methods

    [TestClass]
    public class GetChangeLogTemplate : RepositoryServiceTests
    {
        [TestMethod]
        public void When_RepoFileNotFound_Expect_DefaultEmbeddedResource()
        {
            // arrange
            string fileName = Path.Combine(RepoPath, "changelog_template.md");
            string? contents = null;

            MockFileSystemService.Setup(x => x.TryGetFileContents(fileName, out contents)).Returns(false);

            // act
            Template actual = Subject.GetChangeLogTemplate(RepoPath);

            // assert
            actual.Should().NotBeNull();
        }
    }
}