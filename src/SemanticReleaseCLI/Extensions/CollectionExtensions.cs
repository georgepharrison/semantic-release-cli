namespace SemanticReleaseCLI.Extensions;

internal static class CollectionExtensions
{
    #region Public Methods

    public static int IndexOf<T>(this IReadOnlyList<T> self, T elementToFind)
    {
        int i = 0;

        foreach (T element in self)
        {
            if (Equals(element, elementToFind))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> self)
        => self.ToList().AsReadOnly();

    #endregion Public Methods
}