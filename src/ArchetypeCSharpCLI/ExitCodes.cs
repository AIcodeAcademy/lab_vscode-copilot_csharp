namespace ArchetypeCSharpCLI;

/// <summary>
/// Centralized, stable process exit codes for the CLI.
/// Keep the set small and meaningful for scripting.
/// </summary>
public static class ExitCodes
{
  /// <summary>Operation succeeded.</summary>
  public const int Success = 0;

  /// <summary>Network failure or timeout (DNS, connection refused, TLS, canceled).</summary>
  public const int NetworkOrTimeout = 1;

  /// <summary>Validation error or HTTP 4xx client error.</summary>
  public const int ValidationOrClientError = 2;

  /// <summary>HTTP 5xx server error.</summary>
  public const int ServerError = 3;

  /// <summary>Unexpected unhandled exception.</summary>
  public const int Unexpected = 4;
}
