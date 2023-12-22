using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace SemanticReleaseCLI;

internal sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    #region Private Fields

    private readonly IServiceCollection _builder = builder;

    #endregion Private Fields

    #region Public Methods

    public ITypeResolver Build()
        => new TypeResolver(_builder.BuildServiceProvider());

    [SuppressMessage("Trimming", "IL2067:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.", Justification = "No way to fix this right now")]
    public void Register(Type service, Type implementation)
        => _builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation)
        => _builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        _builder.AddSingleton(service, (provider) => func());
    }

    #endregion Public Methods
}