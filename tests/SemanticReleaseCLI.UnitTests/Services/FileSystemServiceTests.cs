using System.Reflection;
using FluentAssertions;

namespace SemanticReleaseCLI.UnitTests;

public class FileSystemServiceTests
{
    #region Protected Properties

    protected FileSystemService Subject { get; private set; } = default!;

    #endregion Protected Properties

    #region Public Methods

    [TestInitialize]
    public void TestInitialize()
        => Subject = new();

    #endregion Public Methods

    [TestClass]
    public class GetCurrentDirectory : FileSystemServiceTests
    {
        [TestMethod]
        public void Should_ReturnCurrentDirectory()
        {
            // arrange
            string executingAssembly = Assembly.GetExecutingAssembly().Location;
            string expected = Path.GetDirectoryName(executingAssembly) ?? throw new InvalidOperationException();

            // act
            string actual = Directory.GetCurrentDirectory();

            // assert
            actual.Should().Be(expected);
        }
    }
}
