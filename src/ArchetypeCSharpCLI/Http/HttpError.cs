namespace ArchetypeCSharpCLI.Http;

/// <summary>
/// Types of HTTP errors that can occur during API calls.
/// </summary>
public enum HttpErrorType
{
  /// <summary>
  /// Network-related errors (DNS, connection refused, SSL/TLS).
  /// </summary>
  Network,

  /// <summary>
  /// Request timeout errors.
  /// </summary>
  Timeout,

  /// <summary>
  /// HTTP client errors (4xx status codes).
  /// </summary>
  ClientError,

  /// <summary>
  /// HTTP server errors (5xx status codes).
  /// </summary>
  ServerError,

  /// <summary>
  /// Unexpected exceptions during HTTP operations.
  /// </summary>
  Unexpected
}

/// <summary>
/// Represents an HTTP error with user-friendly messaging and exit code mapping.
/// </summary>
public class HttpError
{
  /// <summary>
  /// The type of HTTP error.
  /// </summary>
  public HttpErrorType Type { get; }

  /// <summary>
  /// User-friendly error message.
  /// </summary>
  public string Message { get; }

  /// <summary>
  /// Exit code for the CLI application.
  /// </summary>
  public int ExitCode { get; }

  /// <summary>
  /// The underlying exception, if any.
  /// </summary>
  public Exception? Exception { get; }

  /// <summary>
  /// Correlation ID for tracing the request.
  /// </summary>
  public string? CorrelationId { get; }

  /// <summary>
  /// Initializes a new instance of the HttpError class.
  /// </summary>
  public HttpError(HttpErrorType type, string message, int exitCode, Exception? exception = null, string? correlationId = null)
  {
    Type = type;
    Message = message;
    ExitCode = exitCode;
    Exception = exception;
    CorrelationId = correlationId;
  }

  /// <summary>
  /// Returns a string representation of the error for console output.
  /// </summary>
  public override string ToString()
  {
    var result = $"Error: {Message}";
    if (!string.IsNullOrEmpty(CorrelationId))
    {
      result += $" (Correlation ID: {CorrelationId})";
    }
    return result;
  }
}
