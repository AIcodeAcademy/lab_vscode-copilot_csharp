using ArchetypeCSharpCLI.Commands.Hello;

using ArchetypeCSharpCLI.Logging;
using Microsoft.Extensions.Logging;

namespace ArchetypeCSharpCLI.Commands.Hello;

/// <summary>
/// Implements the behavior of the 'hello' command.
/// </summary>
public static class HelloCommandHandler
{
  /// <summary>
  /// Handles the 'hello' command using the provided options.
  /// </summary>
  /// <param name="opts">Parsed options for the command.</param>
  /// <returns>Exit code (0 on success).</returns>
  public static Task<int> HandleAsync(HelloOptions opts)
  {
    var logger = Log.For(typeof(HelloCommandHandler));
    using var scope = logger.BeginScope("hello name={Name}", opts.Name);
    logger.LogDebug("Handling hello command");
    Console.WriteLine($"Hello, {opts.Name}!");
    return Task.FromResult(0);
  }
}
