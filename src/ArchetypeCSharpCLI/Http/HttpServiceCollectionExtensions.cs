using System.Net.Http.Headers;
using ArchetypeCSharpCLI.Configuration;
using ArchetypeCSharpCLI.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace ArchetypeCSharpCLI.Http;

public static class HttpServiceCollectionExtensions
{
  public static IServiceCollection AddHttpCore(this IServiceCollection services, IConfiguration configuration)
  {
    // Bind minimal AppConfig to read timeout defaults using same precedence as ConfigBuilder
    var temp = new AppConfig();
    configuration.Bind(temp);        // flat keys (e.g., HttpTimeoutSeconds)
    configuration.Bind("App", temp); // nested section overrides (e.g., App:HttpTimeoutSeconds)

    var normalized = new AppConfig
    {
      Environment = string.IsNullOrWhiteSpace(temp.Environment) ? "Production" : temp.Environment,
      // Preserve configured value (may be out of range); downstream clamp enforces [1,60].
      // If not configured, AppConfig default (30) is used.
      HttpTimeoutSeconds = temp.HttpTimeoutSeconds,
      LogLevel = string.IsNullOrWhiteSpace(temp.LogLevel) ? "Information" : temp.LogLevel
    };

    return services.AddHttpCore(normalized);
  }

  public static IServiceCollection AddHttpCore(this IServiceCollection services, AppConfig settings)
  {
    services.AddHttpClient();

    // Register HTTP error handler
    services.AddSingleton<IHttpErrorHandler, HttpErrorHandler>();

    // Register mappers
    services.AddSingleton<IIpApiMapper, IpApiMapper>();
    services.AddSingleton<IOpenMeteoMapper, OpenMeteoMapper>();

    var timeoutSeconds = Clamp(settings.HttpTimeoutSeconds, 1, 60);
    var product = new ProductInfoHeaderValue("ArchetypeCSharpCLI", VersionInfo.GetInformationalVersion());

    services.ConfigureAll<HttpClientFactoryOptions>(options =>
    {
      options.HttpClientActions.Add(client =>
      {
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        // Ensure we don't duplicate User-Agent segments if the factory creates multiple instances
        if (!client.DefaultRequestHeaders.UserAgent.Contains(product))
        {
          client.DefaultRequestHeaders.UserAgent.Add(product);
        }
      });
    });

    // Register GeoIP typed client
    // Base address uses ip-api.com which exposes /json endpoint for caller IP
    services.AddHttpClient<ArchetypeCSharpCLI.Http.GeoIp.IGeoIpClient, ArchetypeCSharpCLI.Http.GeoIp.GeoIpClient>(client =>
      {
        client.BaseAddress = new Uri("http://ip-api.com");
      })
    .AddHttpMessageHandler(() => new HttpLoggingHandler());

    // Register Open-Meteo typed client
    services.AddHttpClient<ArchetypeCSharpCLI.Http.Weather.IWeatherClient, ArchetypeCSharpCLI.Http.Weather.WeatherClient>(client =>
      {
        client.BaseAddress = new Uri("https://api.open-meteo.com");
      })
    .AddHttpMessageHandler(() => new HttpLoggingHandler());

    return services;
  }

  private static int Clamp(int value, int min, int max)
    => value < min ? min : (value > max ? max : value);
}
