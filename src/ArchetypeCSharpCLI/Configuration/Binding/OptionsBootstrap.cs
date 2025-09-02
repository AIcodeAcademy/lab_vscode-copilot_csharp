using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchetypeCSharpCLI.Configuration.Binding;

/// <summary>
/// Minimal bootstrapper to host Options and Configuration without adopting full DI yet.
/// Builds a <see cref="ServiceProvider"/> with Options services and stores it for access via <see cref="OptionsAccess"/>.
/// </summary>
public static class OptionsBootstrap
{
  private static IServiceProvider? _serviceProvider;

  /// <summary>
  /// Exposes the built service provider for simple service location scenarios.
  /// Returns null until <see cref="Init"/> has been called.
  /// </summary>
  public static IServiceProvider? Services => OptionsBootstrapAccessor.ServiceProvider;

  /// <summary>
  /// Initializes the options container using the provided configuration.
  /// Registers <see cref="IConfiguration"/> and options services; callers can extend registrations
  /// using the optional <paramref name="configure"/> callback (e.g., to bind specific option types).
  /// </summary>
  /// <param name="configuration">The configuration root used by options bindings.</param>
  /// <param name="configure">An optional callback to add services and bind options.</param>
  /// <returns>The built <see cref="IServiceProvider"/> for advanced scenarios.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
  public static IServiceProvider Init(IConfiguration configuration, Action<IServiceCollection>? configure = null)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var services = new ServiceCollection();
    services.AddSingleton(configuration);
    services.AddOptions();

    configure?.Invoke(services);

    _serviceProvider = services.BuildServiceProvider();
    OptionsBootstrapAccessor.ServiceProvider = _serviceProvider;
    return _serviceProvider;
  }

  internal static class OptionsBootstrapAccessor
  {
    internal static IServiceProvider? ServiceProvider { get; set; }
  }
}
