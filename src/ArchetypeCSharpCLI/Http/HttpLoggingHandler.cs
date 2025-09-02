using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using ArchetypeCSharpCLI.Logging;

namespace ArchetypeCSharpCLI.Http;

/// <summary>
/// Delegating handler that logs outgoing HTTP requests and responses.
/// - Logs request method and absolute URL at Information level.
/// - Logs response status code at Information level.
/// - On non-success (>=400), logs a short excerpt of the response body to help diagnose errors.
/// </summary>
public class HttpLoggingHandler : DelegatingHandler
{
  private readonly ILogger _logger;

  public HttpLoggingHandler()
  {
    // Use centralized logger factory; category: HTTP
    _logger = Log.For("HTTP");
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    // Build absolute URL if BaseAddress + relative path is used
    var absoluteUri = request.RequestUri?.IsAbsoluteUri == true
      ? request.RequestUri!.ToString()
      : (request.RequestUri is not null && request.Headers.TryGetValues("Host", out var _)
          ? request.RequestUri.ToString()
          : (request.RequestUri?.ToString() ?? "<null>"));

    _logger.LogInformation("HTTP {Method} {Url}", request.Method, absoluteUri);

    HttpResponseMessage? response = null;
    try
    {
      response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
      _logger.LogInformation("HTTP {Status} {Method} {Url}", (int)response.StatusCode, request.Method, absoluteUri);

      if (!response.IsSuccessStatusCode)
      {
        // Try to capture a small portion of the body for diagnostics
        string snippet = await SafeReadBodySnippetAsync(response, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(snippet))
        {
          _logger.LogWarning("HTTP error body (truncated): {Snippet}", snippet);
        }
      }

      return response;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogError(ex, "HTTP request failed: {Method} {Url}", request.Method, absoluteUri);
      throw;
    }
  }

  private static async Task<string> SafeReadBodySnippetAsync(HttpResponseMessage response, CancellationToken ct)
  {
    try
    {
      var content = response.Content;
      if (content is null) return string.Empty;
      var text = await content.ReadAsStringAsync(ct).ConfigureAwait(false);
      if (string.IsNullOrEmpty(text)) return string.Empty;
      const int max = 2000; // cap to avoid flooding logs
      if (text.Length <= max) return text;
      var sb = new StringBuilder(text.AsSpan(0, max).ToString());
      sb.Append("… [truncated]");
      return sb.ToString();
    }
    catch
    {
      return string.Empty;
    }
  }
}
