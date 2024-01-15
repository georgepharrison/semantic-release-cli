using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Services;

namespace SemanticReleaseCLI.UnitTests;

public class GitServiceTests
{
    #region Protected Properties

    protected Mock<IFileSystemService> MockFileSystemService { get; private set; } = default!;
    protected string RepoPath { get; private set; } = string.Empty;
    protected GitService Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    public async Task SeuptGitRepoAsync()
    {
        string fileName = "HelloWorld.txt";

        string path = Path.Combine(RepoPath, fileName);

        File.Create(path);

        await Cli.Wrap("git")
            .WithWorkingDirectory(RepoPath)
            .WithArguments("init")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        await Cli.Wrap("git")
            .WithWorkingDirectory(RepoPath)
            .WithArguments(args => args
                .Add("add")
                .Add(fileName)
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        await Cli.Wrap("git")
            .WithWorkingDirectory(RepoPath)
            .WithArguments(args => args
                .Add("commit")
                .Add("-m")
                .Add("Initial Commit")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        MockFileSystemService = new(MockBehavior.Strict);

        Subject = new(MockFileSystemService.Object);

        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string testDirectory = $"{homeDirectory}/SemanticReleaseTests";

        DirectoryInfo directoryInfo = Directory.CreateDirectory(testDirectory);

        RepoPath = directoryInfo.FullName;
    }

    [TestCleanup]
    public void TestCleanup()
        => Directory.Delete(RepoPath, true);

    #endregion Public Methods

    [TestClass]
    public class IsGitRepoAsync : GitServiceTests
    {
        [TestMethod]
        public async Task When_RepoPathIsNotProvided_AndCurrentDirectoryIsAGitRepo_Expect_True()
        {
            // arrange
            await SeuptGitRepoAsync();

            MockFileSystemService
                .Setup(x => x.GetCurrentDirectory())
                .Returns(RepoPath);

            // act
            bool actual = await Subject.IsGitRepoAsync();

            // assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public async Task When_RepoPathIsNotProvided_AndCurrentDirectoryIsNotAGitRepo_Expect_False()
        {
            // arrange
            MockFileSystemService
                .Setup(x => x.GetCurrentDirectory())
                .Returns(RepoPath);

            // act
            bool actual = await Subject.IsGitRepoAsync();

            // assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public async Task When_RepoPathIsAGitRepo_Expect_True()
        {
            // arrange
            await SeuptGitRepoAsync();

            // act
            bool actual = await Subject.IsGitRepoAsync(repoPath: RepoPath);

            // assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public async Task When_RepoPathIsNotAGitRepo_Expect_False()
        {
            // arrange
            // act
            bool actual = await Subject.IsGitRepoAsync(repoPath: RepoPath);

            // assert
            actual.Should().BeFalse();
        }
    }
}
