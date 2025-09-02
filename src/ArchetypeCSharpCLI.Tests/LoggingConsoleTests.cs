using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class LoggingConsoleTests
{
  private static string GetBuiltExecutablePath()
  {
    var configuration = "Debug"; // adjust if needed
    var tfm = "net9.0";
    var binDir = Path.Combine(TestProjectRoot(), "ArchetypeCSharpCLI", "bin", configuration, tfm);
    var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "archetype.exe" : "archetype";
    return Path.Combine(binDir, exe);
  }

  private static string TestProjectRoot()
  {
    // Walk up from test assembly folder to src folder
    var dir = Path.GetDirectoryName(typeof(LoggingConsoleTests).Assembly.Location)!;
    while (dir is not null && !Directory.Exists(Path.Combine(dir, "ArchetypeCSharpCLI")))
    {
      var parent = Directory.GetParent(dir);
      dir = parent?.FullName;
    }
    return dir!;
  }

  private static (int exitCode, string stdout, string stderr) RunCli(
      string[] args,
      (string key, string value)[]? environment = null)
  {
    var exePath = GetBuiltExecutablePath();
    File.Exists(exePath).Should().BeTrue($"CLI executable should exist at {exePath}. Build before running tests.");

    var psi = new ProcessStartInfo
    {
      FileName = exePath,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true,
    };

    if (environment is { Length: > 0 })
    {
      foreach (var (k, v) in environment)
        psi.Environment[k] = v;
    }

    foreach (var a in args)
      psi.ArgumentList.Add(a);

    using var proc = Process.Start(psi)!;
    var stdout = proc.StandardOutput.ReadToEnd();
    var stderr = proc.StandardError.ReadToEnd();
    proc.WaitForExit(10_000).Should().BeTrue("process should exit promptly");
    return (proc.ExitCode, stdout, stderr);
  }

  [Fact]
  public void Default_Level_Does_Not_Print_Debug()
  {
    var (code, stdout, stderr) = RunCli(new[] { "hello", "--name", "Alice" });
    code.Should().Be(0);
    stderr.Should().BeEmpty();
    stdout.Trim().Should().Be("Hello, Alice!");
    stdout.Should().NotContain("Handling hello command");
  }

  [Fact]
  public void Debug_Level_Prints_Debug_With_Scopes_And_Timestamp()
  {
    var (code, stdout, stderr) = RunCli(
        new[] { "hello", "--name", "Bob" },
        new[] { ("App__LogLevel", "Debug"), ("LogLevel", "Debug") }
    );

    code.Should().Be(0);
    // Greeting should be present on stdout
    stdout.Should().Contain("Hello, Bob!");
    // Logs may go to stdout or stderr depending on platform/settings
    var combined = stdout + stderr;
    combined.ToLowerInvariant().Should().Contain(" dbug:");

    // Scopes from middleware and handler
    combined.Should().Contain("cmd=hello");
    combined.Should().Contain("args=");
    combined.Should().Contain("--name Bob");
    combined.Should().Contain("hello name=Bob");

    // A timestamp like HH:mm:ss at the start of some line
    var anyLine = (stdout + stderr).Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
        .First(l => l.ToLowerInvariant().Contains(" dbug:"));
    Regex.IsMatch(anyLine, @"\b\d{2}:\d{2}:\d{2} ").Should().BeTrue();
  }

  [Fact]
  public void Invalid_LogLevel_Defaults_To_Information_And_Suppresses_Debug()
  {
    var (code, stdout, stderr) = RunCli(
        new[] { "hello", "--name", "Charlie" },
        new[] { ("App__LogLevel", "NotALevel") }
    );

    code.Should().Be(0);
    stderr.Should().BeEmpty();
    stdout.Trim().Should().Be("Hello, Charlie!");
    (stdout + stderr).Should().NotContain("Handling hello command");
  }

  [Fact]
  public void Flat_LogLevel_Variable_Is_Also_Respected()
  {
    var (code, stdout, stderr) = RunCli(
        new[] { "hello", "--name", "Dana" },
        new[] { ("LogLevel", "Debug") }
    );

    code.Should().Be(0);
    stderr.Should().BeEmpty();
    stdout.Should().Contain("Hello, Dana!");
    (stdout + stderr).Should().Contain("Handling hello command");
  }
}
