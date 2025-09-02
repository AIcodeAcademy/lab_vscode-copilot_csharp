using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchetypeCSharpCLI.Configuration.Binding;

/// <summary>
/// Extensions to register bound and validated options.
/// </summary>
public static class OptionsExtensions
{
  /// <summary>
  /// Binds a configuration section to an options type, adds DataAnnotations validation,
  /// and allows optional post-configuration and custom validation predicate.
  /// </summary>
  /// <typeparam name="T">The options POCO type to bind and validate.</typeparam>
  /// <param name="services">The service collection to register options into.</param>
  /// <param name="configuration">The configuration root that contains the section.</param>
  /// <param name="sectionName">The configuration section name to bind (e.g., "App").</param>
  /// <param name="postConfigure">Optional action to post-configure bound options.</param>
  /// <param name="validate">Optional predicate for custom validation; return true when valid.</param>
  /// <param name="validateError">Optional custom error message when <paramref name="validate"/> fails.</param>
  /// <returns>The same <see cref="IServiceCollection"/> to allow fluent configuration.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="sectionName"/> is null or whitespace.</exception>
  public static IServiceCollection AddBoundOptions<T>(this IServiceCollection services, IConfiguration configuration, string sectionName,
      Action<T>? postConfigure = null, Func<T, bool>? validate = null, string? validateError = null)
      where T : class, new()
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);
    if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentException("Section name is required", nameof(sectionName));

    services
        .AddOptions<T>()
        .Bind(configuration.GetSection(sectionName))
        .ValidateDataAnnotations();

    if (postConfigure is not null)
    {
      services.PostConfigure(postConfigure);
    }

    if (validate is not null)
    {
      services.PostConfigure<T>(o =>
      {
        if (!validate(o))
        {
          throw new OptionsValidationException(typeof(T).Name, typeof(T), new[] { validateError ?? "custom validation failed" });
        }
      });
    }

    return services;
  }
}
