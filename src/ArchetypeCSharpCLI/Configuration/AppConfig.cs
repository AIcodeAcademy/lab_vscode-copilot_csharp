namespace ArchetypeCSharpCLI.Configuration;

/// <summary>
/// Typed application configuration with safe defaults.
/// </summary>
public sealed class AppConfig
{
    public string Environment { get; init; } = "Production";
    public int HttpTimeoutSeconds { get; init; } = 30; // clamp 1..300
    public string LogLevel { get; init; } = "Information";
}
