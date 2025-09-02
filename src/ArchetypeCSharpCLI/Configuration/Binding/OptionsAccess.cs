using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchetypeCSharpCLI.Configuration.Binding;

/// <summary>
/// Static access to options services backed by the bootstrap service provider.
/// </summary>
public static class OptionsAccess
{
  private static IServiceProvider? ServiceProvider => OptionsBootstrap.OptionsBootstrapAccessor.ServiceProvider;

  /// <summary>
  /// Resolves <see cref="IOptions{TOptions}"/> for the specified options type.
  /// Requires a prior call to <see cref="OptionsBootstrap.Init(Microsoft.Extensions.Configuration.IConfiguration, System.Action{Microsoft.Extensions.DependencyInjection.IServiceCollection}?)"/>.
  /// </summary>
  /// <typeparam name="T">The options type to resolve.</typeparam>
  /// <returns>An <see cref="IOptions{TOptions}"/> instance for <typeparamref name="T"/>.</returns>
  /// <exception cref="InvalidOperationException">Thrown when the bootstrapper wasn't initialized.</exception>
  public static IOptions<T> Get<T>() where T : class
  {
    if (ServiceProvider is null)
      throw new InvalidOperationException("OptionsBootstrap.Init must be called before accessing options.");
    return ServiceProvider.GetRequiredService<IOptions<T>>();
  }

  /// <summary>
  /// Resolves <see cref="IOptionsMonitor{TOptions}"/> to observe live updates of the options.
  /// Requires a prior call to <see cref="OptionsBootstrap.Init(Microsoft.Extensions.Configuration.IConfiguration, System.Action{Microsoft.Extensions.DependencyInjection.IServiceCollection}?)"/>.
  /// </summary>
  /// <typeparam name="T">The options type to monitor.</typeparam>
  /// <returns>An <see cref="IOptionsMonitor{TOptions}"/> instance for <typeparamref name="T"/>.</returns>
  /// <exception cref="InvalidOperationException">Thrown when the bootstrapper wasn't initialized.</exception>
  public static IOptionsMonitor<T> GetMonitor<T>() where T : class
  {
    if (ServiceProvider is null)
      throw new InvalidOperationException("OptionsBootstrap.Init must be called before accessing options.");
    return ServiceProvider.GetRequiredService<IOptionsMonitor<T>>();
  }
}
