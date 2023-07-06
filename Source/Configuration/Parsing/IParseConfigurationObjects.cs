using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace IoCExtensions.Configuration.Parsing;

/// <summary>
/// Defines a parser for a IoCExtensions <see cref="IConfigurationSection"/>.
/// </summary>
public interface IParseConfigurationObjects
{
    /// <summary>
    /// Tries to parse the <see cref="IConfigurationSection"/> to the specified <typeparamref name="TOptions"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfigurationSection"/> to parse.</param>
    /// <param name="parsed">The outputted <typeparamref name="TOptions"/>.</param>
    /// <param name="error">The outputted <see cref="Exception"/>.</param>
    /// <typeparam name="TOptions">The type of the configuration.</typeparam>
    /// <returns>True if successfully parsed, false if not.</returns>
    bool TryParseFrom<TOptions>(IConfigurationSection configuration, [NotNullWhen(true)]out TOptions? parsed, [NotNullWhen(false)]out Exception? error)
        where TOptions : class;
}
