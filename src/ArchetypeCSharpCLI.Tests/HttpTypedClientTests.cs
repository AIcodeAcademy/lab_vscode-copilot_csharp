using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ArchetypeCSharpCLI.Configuration;
using ArchetypeCSharpCLI.Configuration.Binding;
using ArchetypeCSharpCLI.Http;
using ArchetypeCSharpCLI.Tests.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class HttpTypedClientTests
{
  private static IServiceProvider BuildServicesWithConfig(IConfiguration configuration, Action<IServiceCollection>? extra = null)
  {
    return OptionsBootstrap.Init(configuration, services =>
    {
      services.AddHttpCore(configuration);
      extra?.Invoke(services);
    });
  }

  private static IConfigurationRoot BuildConfig(string? env = null)
  {
    if (!string.IsNullOrWhiteSpace(env))
      Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", env);

    return new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
      .AddJsonFile($"appsettings.{(env ?? "Production")}.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables()
      .Build();
  }

  [Fact]
  public void Registers_IHttpClientFactory_And_Defaults()
  {
    using var _files = TempSettingsFiles.Create();
    var cfg = BuildConfig();
    var sp = BuildServicesWithConfig(cfg);

    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient();

    // Default timeout should be 30s when not configured
    client.Timeout.Should().Be(TimeSpan.FromSeconds(30));

    // User-Agent should include product token with version (informational version allowed)
    var uaString = string.Join(" ", client.DefaultRequestHeaders.UserAgent.Select(p => p.ToString()));
    uaString.Should().MatchRegex(@"\bArchetypeCSharpCLI/\S+");
  }

  private interface IExampleApi
  {
    int TimeoutSeconds { get; }
    string UserAgentString { get; }
  }

  private sealed class ExampleApi : IExampleApi
  {
    public int TimeoutSeconds { get; }
    public string UserAgentString { get; }

    public ExampleApi(HttpClient http)
    {
      TimeoutSeconds = (int)Math.Round(http.Timeout.TotalSeconds);
      UserAgentString = string.Join(" ", http.DefaultRequestHeaders.UserAgent.Select(p => p.ToString()));
    }
  }

  [Theory]
  [InlineData(10, 10)]
  [InlineData(0, 1)]    // clamp low
  [InlineData(100, 60)] // clamp high
  public void Typed_Clients_Inherit_Timeout_And_Headers(int configured, int expected)
  {
    using var _files = TempSettingsFiles.Create(new { App = new { HttpTimeoutSeconds = configured } });
    var cfg = BuildConfig();

    var sp = BuildServicesWithConfig(cfg, services =>
    {
      services.AddHttpClient<IExampleApi, ExampleApi>();
    });

    var api = sp.GetRequiredService<IExampleApi>();
    api.TimeoutSeconds.Should().Be(expected);
    api.UserAgentString.Should().MatchRegex(@"\bArchetypeCSharpCLI/\S+");
  }

  [Fact]
  public void Environment_File_Precedes_Env_Var_Overrides()
  {
    using var _envVar = new EnvVarScope(("App__HttpTimeoutSeconds", "25"));
    using var _files = TempSettingsFiles.Create(
      appsettings: new { App = new { HttpTimeoutSeconds = 5 } },
      appsettingsEnv: new { App = new { HttpTimeoutSeconds = 12 } },
      envName: "Development");

    var cfg = BuildConfig("Development");
    var sp = BuildServicesWithConfig(cfg);

    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    // Environment variable should override env-specific file
    client.Timeout.Should().Be(TimeSpan.FromSeconds(25));
  }

  [Fact]
  public void File_Env_Specific_Applies_When_No_Env_Var()
  {
    using var _files = TempSettingsFiles.Create(
      appsettings: new { App = new { HttpTimeoutSeconds = 5 } },
      appsettingsEnv: new { App = new { HttpTimeoutSeconds = 12 } },
      envName: "Development");

    var cfg = BuildConfig("Development");
    var sp = BuildServicesWithConfig(cfg);

    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    client.Timeout.Should().Be(TimeSpan.FromSeconds(12));
  }
}
