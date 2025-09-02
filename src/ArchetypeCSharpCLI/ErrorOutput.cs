namespace ArchetypeCSharpCLI;

using ArchetypeCSharpCLI.Http;

/// <summary>
/// Helper for concise, user-friendly error messages to stderr.
/// </summary>
public static class ErrorOutput
{
  /// <summary>
  /// Writes a user-friendly message to stderr. Includes correlation when provided.
  /// </summary>
  public static void Write(string message, string? correlationId = null)
  {
    if (!string.IsNullOrWhiteSpace(correlationId))
      Console.Error.WriteLine($"Error: {message} (Correlation ID: {correlationId})");
    else
      Console.Error.WriteLine($"Error: {message}");
  }

  /// <summary>
  /// Writes a user-friendly message based on HttpError.
  /// </summary>
  public static void Write(HttpError error)
    => Write(error.Message, error.CorrelationId);
}
