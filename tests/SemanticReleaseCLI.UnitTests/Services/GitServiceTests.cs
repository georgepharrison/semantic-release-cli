using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Services;

namespace SemanticReleaseCLI.UnitTests;

public class GitServiceTests
{
    #region Private Fields

    private readonly string _fileName = "HelloWorld.txt";
    private string _path = string.Empty;

    #endregion Private Fields

    #region Protected Properties

    protected Mock<IFileSystemService> MockFileSystemService { get; private set; } = default!;
    protected string RepoPath { get; private set; } = string.Empty;
    protected GitService Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    public async Task SeuptGitRepoAsync()
    {
        await CreateCommit("Initial Commit", "2023-12-23 00:00:00 +0000", initialCommit: true);
        await CreateCommit("feat(web): first feature", "2024-01-01 00:00:00 +0000");
        await CreateCommit("feat(web): second feature", "2024-01-01 01:00:00 +0000");
        await CreateCommit("feat(web): third feature", "2024-01-01 03:00:00 +0000");
        await CreateCommit("feat(cli): first feature", "2024-01-01 04:00:00 +0000");
        await CreateCommit("feat(cli): second feature", "2024-01-01 05:00:00 +0000");
        await CreateCommit("feat(cli): third feature", "2024-01-01 06:00:00 +0000");
        await CreateCommit("chore(CHANGELOG): 0.24.101.1", "2024-01-01 12:00:00 +0000", "0.24.101.1");
        await CreateCommit("fix(cli): first fix", "2024-01-09 00:00:00 +0000");
        await CreateCommit("fix(cli): second fix", "2024-01-09 00:00:00 +0000");
        await CreateCommit("feat(web): feature with breaking change\r\n\r\nHere's a description of what happened\r\n\r\nBREAKING CHANGE: this will break your stuff", "2024-01-09 00:00:00 +0000");
        await CreateCommit("feat(web): bump the version\r\n\r\nThis is a body\r\n\r\nBUMP VERSION", "2024-01-10 00:00:00 +0000");
        await CreateCommit("chore(CHANGELOG): 2.24.110.1", "2024-01-10 00:00:00 +0000");
        await CreateCommit("feat: bump the version\r\n\r\nBUMP VERSION", "2024-01-11 00:00:00 +0000");
        await CreateCommit("feat: some new feature", "2024-01-15 00:00:00 +0000");
        await CreateCommit("fix: some fix", "2024-01-15 01:00:00 +0000");
        await CreateCommit("chore(CHANGELOG): 3.24.115.2", "2024-01-01 00:00:00 +0000", "3.24.115.2");

        string commitMessage =
            $"feat: this has body and footers\r\n" +
            $"\r\n" +
            $"This is a body that has multiple paragraphs.\r\n" +
            $"This is the second line to the first paragraph.\r\n" +
            $"\r\n" +
            $"This is the second paragraph to the body.\r\n" +
            $"This is the second line to the second paragraph.\r\n" +
            $"\r\n" +
            $"This is the third paragraph to the body.\r\n" +
            $"This is the second line to the third paragraph.\r\n" +
            $"This is the third line to the third paragraph.\r\n" +
            $"\r\n" +
            $"Change: 24\r\n" +
            $"\r\n" +
            $"Reviewed-by: George\r\n" +
            $"Reviewed-at: 12/24/2023\r\n" +
            $"\r\n" +
            $"ReviewNumber #23\r\n" +
            $"BREAKING CHANGE: API now has a new parameter for the method";

        await CreateCommit(commitMessage, "2024-01-17 00:00:00 +0000");
        await CreateCommit("chore(CHANGELOG): 4.24.117.1", "2024-01-17 00:00:00 +0000", "4.24.117.1");
        await CreateCommit("feat: some new feature\n\nBUMP VERSION", "2024-01-19 00:00:00 +0000");
        await CreateCommit("fix: some fix\n\nBREAKING CHANGE: broken", "2024-01-20 00:00:00 +0000");
        await CreateCommit("chore: tech debt", "2024-01-20 01:00:00 +0000");
    }

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

        void RemoveDirectory(DirectoryInfo directory)
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
        MockFileSystemService = new(MockBehavior.Strict);

        Subject = new(MockFileSystemService.Object);

        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string testDirectory = $"{homeDirectory}/SemanticReleaseTests";

        DirectoryInfo directoryInfo = Directory.CreateDirectory(testDirectory);

        RepoPath = directoryInfo.FullName;

        _path = Path.Combine(RepoPath, _fileName);
    }

    #endregion Public Methods

    #region Private Methods

    private async Task CreateCommit(string commitMessage, string authorDate, string? tag = null, bool initialCommit = false)
    {
        if (initialCommit)
        {
            await Cli.Wrap("git")
                .WithWorkingDirectory(RepoPath)
                .WithArguments("init")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
        }

        await File.AppendAllTextAsync(_path, "Edit");

        await Cli.Wrap("git")
            .WithWorkingDirectory(RepoPath)
            .WithArguments(args => args
                .Add("add")
                .Add(_fileName)
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        await Cli.Wrap("git")
            .WithWorkingDirectory(RepoPath)
            .WithArguments(args => args
                .Add("commit")
                .Add("-m")
                .Add(commitMessage)
                .Add($"--date={authorDate}")
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        if (tag is not null)
        {
            await Cli.Wrap("git")
                .WithWorkingDirectory(RepoPath)
                .WithArguments(args => args
                    .Add("tag")
                    .Add(tag)
                )
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
        }
    }

    #endregion Private Methods

    #region Public Classes

    [TestClass]
    public class IsGitRepoAsync : GitServiceTests
    {
        #region Public Methods

        [TestMethod]
        public async Task When_RepoPathIsAGitRepo_Expect_True()
        {
            // arrange
            await SeuptGitRepoAsync();

            // act
            bool actual = await Subject.IsGitRepoAsync(repoPath: RepoPath);

            // assert
            actual.Should().BeTrue();

            RepositoryService repositoryService = new(MockFileSystemService.Object, Subject);

            IReadOnlyList<Release> releases = await repositoryService.GetReleasesAsync(RepoPath);
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

        #endregion Public Methods
    }

    #endregion Public Classes
}
