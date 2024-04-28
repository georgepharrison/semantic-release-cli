using FluentAssertions;
using System.Reflection;

namespace SemanticReleaseCLI.UnitTests;

public class FileSystemServiceTests
{
    #region Private Fields

    private string _testDirectory = string.Empty;

    #endregion Private Fields

    #region Protected Properties

    protected FileSystemService Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    [TestCleanup]
    public void TestCleanup()
        => Directory.Delete(_testDirectory, true);

    [TestInitialize]
    public void TestInitialize()
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string testDirectory = $"{homeDirectory}/SemanticReleaseTests";

        DirectoryInfo directoryInfo = Directory.CreateDirectory(testDirectory);

        _testDirectory = directoryInfo.FullName;

        Subject = new();
    }

    #endregion Public Methods

    #region Public Classes

    [TestClass]
    public class GetCurrentDirectory : FileSystemServiceTests
    {
        #region Public Methods

        [TestMethod]
        public void Should_ReturnCurrentDirectory()
        {
            // arrange
            string executingAssembly = Assembly.GetExecutingAssembly().Location;
            string expected = Path.GetDirectoryName(executingAssembly) ?? throw new InvalidOperationException();

            // act
            string actual = Subject.GetCurrentDirectory();

            // assert
            actual.Should().Be(expected);
        }

        #endregion Public Methods
    }

    [TestClass]
    public class TryGetFileContents : FileSystemServiceTests
    {
        #region Public Methods

        [TestMethod]
        public void When_FileFound_Expect_True()
        {
            // arrange
            string expectedContents = "Hello World!";
            string fileName = Path.Combine(_testDirectory, "NotFound");

            File.WriteAllText(fileName, expectedContents);

            // act
            bool actual = Subject.TryGetFileContents(fileName, out string? contents);

            // assert
            actual.Should().BeTrue();
            contents.Should().NotBeNull();
            contents.Should().BeEquivalentTo(expectedContents);
        }

        [TestMethod]
        public void When_FileNotFound_Expect_False()
        {
            // arrange
            string fileName = Path.Combine(_testDirectory, "NotFound");

            // act
            bool actual = Subject.TryGetFileContents(fileName, out string? contents);

            // assert
            actual.Should().BeFalse();
            contents.Should().BeNull();
        }

        #endregion Public Methods
    }

    #endregion Public Classes
}
