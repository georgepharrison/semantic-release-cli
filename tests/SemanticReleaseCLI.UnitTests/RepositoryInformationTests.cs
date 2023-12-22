using FluentAssertions;
using Moq;
using SemanticReleaseCLI.Interfaces;

namespace SemanticReleaseCLI.UnitTests;

public class RepositoryInformationTests
{
    #region Protected Properties

    protected Mock<IGitService> MockGitService { get; private set; } = default!;
    protected Mock<IReleaseCliService> MockReleaseCliService { get; private set; } = default!;
    protected RepositoryInformation Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    [TestInitialize]
    public void TestInitialize()
    {
        MockGitService = new(MockBehavior.Strict);
        MockReleaseCliService = new(MockBehavior.Strict);

        Subject = new(MockGitService.Object, MockReleaseCliService.Object);
    }

    #endregion Public Methods

    #region Public Classes

    [TestClass]
    public class GetCommitTag : RepositoryInformationTests
    {
        #region Public Methods

        [TestMethod]
        public async Task When_NoTagsFound_Expect_0_YY_MMDD_ChangeOfDay()
        {
            // arrange
            static async IAsyncEnumerable<string> GetTags()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield break;
            }

            static async IAsyncEnumerable<string> SearchLogs()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield break;
            }

            string workingDirectory = "/work";
            string? tagAthead = null;
            string? previousTag = null;
            string latestCommitDate = "2023-12-25";
            int commits = 24;

            MockGitService.Setup(x => x.GetTagAtHead(null, workingDirectory))
                .ReturnsAsync(tagAthead);

            MockGitService.Setup(x => x.GetTagBeforeHead(null, workingDirectory))
                .ReturnsAsync(previousTag);

            MockGitService.Setup(x => x.GetTags("committerdate", null, workingDirectory))
                .Returns(GetTags);

            MockGitService.Setup(x => x.SearchLogs(@"BUMP\sVERSION", null, null, null, null, workingDirectory))
                .Returns(SearchLogs);

            MockGitService.Setup(x => x.GetCommitDate("%cs", null, null, workingDirectory))
                .ReturnsAsync(latestCommitDate);

            MockGitService.Setup(x => x.GetCommitCount($"{latestCommitDate}T00:00:00", $"{latestCommitDate}T23:59:59", null, workingDirectory))
                .ReturnsAsync(commits);

            // act
            string actual = await Subject.GetCommitTag(workingDirectory);

            // assert
            actual.Should().Be("0.23.1225.24");
        }

        [TestMethod]
        public async Task When_NoTagsFoundAndLogWithBumpVersion_Expect_1_YY_MMDD_ChangeofDay()
        {
            // arrange
            static async IAsyncEnumerable<string> GetTags()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield break;
            }

            static async IAsyncEnumerable<string> SearchLogs()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield return "feat: new feature";
                yield return @"feat: new feature\nBUMP VERSION";
                yield return "chore: refactor";
            }

            string workingDirectory = "/work";
            string? tagAthead = null;
            string? previousTag = null;
            string latestCommitDate = "2023-12-25";
            int commits = 24;

            MockGitService.Setup(x => x.GetTagAtHead(null, workingDirectory))
                .ReturnsAsync(tagAthead);

            MockGitService.Setup(x => x.GetTagBeforeHead(null, workingDirectory))
                .ReturnsAsync(previousTag);

            MockGitService.Setup(x => x.GetTags("committerdate", null, workingDirectory))
                .Returns(GetTags);

            MockGitService.Setup(x => x.SearchLogs(@"BUMP\sVERSION", null, null, null, null, workingDirectory))
                .Returns(SearchLogs);

            MockGitService.Setup(x => x.GetCommitDate("%cs", null, null, workingDirectory))
                .ReturnsAsync(latestCommitDate);

            MockGitService.Setup(x => x.GetCommitCount($"{latestCommitDate}T00:00:00", $"{latestCommitDate}T23:59:59", null, workingDirectory))
                .ReturnsAsync(commits);

            // act
            string actual = await Subject.GetCommitTag(workingDirectory);

            // assert
            actual.Should().Be("1.23.1225.24");
        }

        [TestMethod]
        public async Task When_TagAtHead_Expect_TagAtHead()
        {
            // arrange
            string workingDirectory = "/home";
            string? tagAtHead = "1.0";

            MockGitService.Setup(x => x.GetTagAtHead(null, workingDirectory))
                .ReturnsAsync(tagAtHead);

            // act
            string actual = await Subject.GetCommitTag(workingDirectory);

            // assert
            actual.Should().Be(tagAtHead);
        }

        [TestMethod]
        public async Task When_TagsFound_Expect_PreviousTagMajor_YY_MMDD_ChangeOfDay()
        {
            // arrange
            static async IAsyncEnumerable<string> GetTags()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield return "3.0";
            }

            static async IAsyncEnumerable<string> SearchLogs()
            {
                await Task.Yield(); // to make the compiler warning go away

                yield break;
            }

            string workingDirectory = "/work";
            string? tagAthead = null;
            string? previousTag = "3.0";
            string latestCommitDate = "2023-12-25";
            int commits = 24;

            MockGitService.Setup(x => x.GetTagAtHead(null, workingDirectory))
                .ReturnsAsync(tagAthead);

            MockGitService.Setup(x => x.GetTagBeforeHead(null, workingDirectory))
                .ReturnsAsync(previousTag);

            MockGitService.Setup(x => x.GetTags("committerdate", null, workingDirectory))
                .Returns(GetTags);

            MockGitService.Setup(x => x.SearchLogs(@"BUMP\sVERSION", "P", "3.0", "HEAD", null, workingDirectory))
                .Returns(SearchLogs);

            MockGitService.Setup(x => x.GetCommitDate("%cs", null, null, workingDirectory))
                .ReturnsAsync(latestCommitDate);

            MockGitService.Setup(x => x.GetCommitCount($"{latestCommitDate}T00:00:00", $"{latestCommitDate}T23:59:59", null, workingDirectory))
                .ReturnsAsync(commits);

            // act
            string actual = await Subject.GetCommitTag(workingDirectory);

            // assert
            actual.Should().Be("3.23.1225.24");
        }

        #endregion Public Methods
    }

    #endregion Public Classes
}