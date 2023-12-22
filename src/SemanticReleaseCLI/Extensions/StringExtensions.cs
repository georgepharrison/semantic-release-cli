namespace SemanticReleaseCLI.Extensions;

internal static class StringExtensions
{
    #region Public Methods

    public static string FirstCharToUpper(this string input)
        => input switch
        {
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpperInvariant(), input.AsSpan(1))
        };

    public static IEnumerable<string> SplitToLines(this string input)
    {
        if (input is null)
        {
            yield break;
        }

        using StringReader reader = new(input);

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            yield return line;
        }
    }

    #endregion Public Methods
}
