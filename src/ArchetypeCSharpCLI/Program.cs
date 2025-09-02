using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;
using ArchetypeCSharpCLI.Commands;
using ArchetypeCSharpCLI.Configuration;
using ArchetypeCSharpCLI.Configuration.Binding;

namespace ArchetypeCSharpCLI;

public static class Program
{
  /// <summary>
  /// Entry point for the Archetype C# CLI. Builds the command-line parser and
  /// delegates execution. Defaults to showing help when no arguments are provided.
  /// </summary>
  /// <param name="args">Command-line arguments passed to the application.</param>
  /// <returns>Process exit code (0 for success; non-zero on failures).</returns>
  public static int Main(string[] args)
  {
    // Initialize configuration early; safe no-op if files missing
    var rawConfig = ConfigBuilder.BuildRaw();
    // Initialize options container (no specific types bound yet). Keeps future DI smooth.
    OptionsBootstrap.Init(rawConfig, services =>
    {
      // Register HttpClientFactory and default HTTP settings based on AppConfig
      ArchetypeCSharpCLI.Http.HttpServiceCollectionExtensions.AddHttpCore(services, rawConfig);
    });
    // Preserve existing typed settings cache behavior
    _ = AppSettings.Current;
    var parser = BuildParser();
    // Show help by default when no args are provided
    var argList = args is { Length: > 0 } ? args : new[] { "--help" };
    return parser.InvokeAsync(argList).GetAwaiter().GetResult();
  }

  /// <summary>
  /// Gets the semantic version for this application, preferring the
  /// <see cref="AssemblyInformationalVersionAttribute"/> value when present.
  /// </summary>
  /// <returns>The application version string.</returns>
  private static string GetInformationalVersion()
  {
    var asm = Assembly.GetExecutingAssembly();
    var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    return string.IsNullOrWhiteSpace(info)
        ? asm.GetName().Version?.ToString() ?? "0.0.0"
        : info;
  }

  /// <summary>
  /// Builds the System.CommandLine parser and wires common behaviors:
  /// default help, <c>--version</c> (<c>-v</c>) printing, and help precedence.
  /// </summary>
  /// <returns>A configured <see cref="Parser"/> ready to invoke.</returns>
  private static Parser BuildParser()
  {
    return CommandFactory.BuildParser(GetInformationalVersion);
  }
}
