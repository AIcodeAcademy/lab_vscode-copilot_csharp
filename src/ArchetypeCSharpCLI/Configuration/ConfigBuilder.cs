using Microsoft.Extensions.Configuration;

namespace ArchetypeCSharpCLI.Configuration;

/// <summary>
/// Builds IConfiguration and typed AppConfig from files and environment variables.
/// </summary>
public static class ConfigBuilder
{
  public static IConfigurationRoot BuildRaw()
  {
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
              ?? "Production";

    var builder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
  .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables();

    return builder.Build();
  }

  public static AppConfig Build()
  {
    var raw = BuildRaw();
    var cfg = new AppConfig();
    // Bind to a temp object, then normalize
    var temp = new AppConfig();
    // First bind flat keys, then allow overrides from nested "App" section
    // to ensure environment variables like App__HttpTimeoutSeconds win
    raw.Bind(temp);           // flat keys (e.g., HttpTimeoutSeconds)
    raw.Bind("App", temp);   // nested section overrides (e.g., App:HttpTimeoutSeconds)

    var httpTimeout = temp.HttpTimeoutSeconds;
    if (httpTimeout <= 0 || httpTimeout > 300) httpTimeout = 30;

    return new AppConfig
    {
      Environment = string.IsNullOrWhiteSpace(temp.Environment) ? "Production" : temp.Environment,
      HttpTimeoutSeconds = httpTimeout,
      LogLevel = string.IsNullOrWhiteSpace(temp.LogLevel) ? "Information" : temp.LogLevel
    };
  }
}
