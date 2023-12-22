using Spectre.Console.Cli;

namespace SemanticReleaseCLI;

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    #region Private Fields

    private readonly IServiceProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    #endregion Private Fields

    #region Public Methods

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return _provider.GetService(type);
    }

    #endregion Public Methods
}