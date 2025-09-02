using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Http;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class HttpErrorHandlingTests
{
  private sealed class StubHandler : HttpMessageHandler
  {
    private readonly HttpResponseMessage _response;
    public StubHandler(HttpResponseMessage response) => _response = response;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      => Task.FromResult(_response);
  }

  [Fact]
  public async Task GetWithErrorHandlingAsync_NonSuccess_ThrowsHttpRequestException()
  {
    var response = new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = "Not Found" };
    var handler = new StubHandler(response);
    var client = new HttpClient(handler);
    var logger = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug)).CreateLogger("test");
    var errorHandler = new HttpErrorHandler(LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HttpErrorHandler>());

    await Assert.ThrowsAsync<HttpRequestException>(async () =>
    {
      await client.GetWithErrorHandlingAsync("https://example.test/404", errorHandler, logger);
    });
  }

  [Fact]
  public void HandleException_SocketException_MapsToNetworkExit1()
  {
    var handler = new HttpErrorHandler(LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HttpErrorHandler>());
    var ex = new HttpRequestException("DNS fail", new SocketException((int)SocketError.HostNotFound));
    var err = handler.HandleException(ex);
    err.Type.Should().Be(HttpErrorType.Network);
    err.ExitCode.Should().Be(1);
    err.Message.Should().Contain("Network").And.Contain("connection");
  }

  [Fact]
  public void HandleException_TaskCanceled_MapsToTimeoutExit1()
  {
    var handler = new HttpErrorHandler(LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HttpErrorHandler>());
    var err = handler.HandleException(new TaskCanceledException());
    err.Type.Should().Be(HttpErrorType.Timeout);
    err.ExitCode.Should().Be(1);
    err.Message.Should().Contain("timed out");
  }

  [Theory]
  [InlineData(HttpStatusCode.BadRequest, HttpErrorType.ClientError, 2)]
  [InlineData(HttpStatusCode.NotFound, HttpErrorType.ClientError, 2)]
  [InlineData(HttpStatusCode.InternalServerError, HttpErrorType.ServerError, 3)]
  public void HandleHttpResponse_MapsStatusCodes(HttpStatusCode status, HttpErrorType expectedType, int expectedExit)
  {
    var handler = new HttpErrorHandler(LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HttpErrorHandler>());
    var resp = new HttpResponseMessage(status) { ReasonPhrase = status.ToString() };
    var err = handler.HandleHttpResponse(resp);
    err.Type.Should().Be(expectedType);
    err.ExitCode.Should().Be(expectedExit);
  }
}
