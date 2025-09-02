using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class CommandRoutingTests
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
        var dir = Path.GetDirectoryName(typeof(CommandRoutingTests).Assembly.Location)!;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "ArchetypeCSharpCLI")))
        {
            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }
        return dir!;
    }

    private static (int exitCode, string stdout, string stderr) RunCli(params string[] args)
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
        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit(10_000).Should().BeTrue("process should exit promptly");
        return (proc.ExitCode, stdout, stderr);
    }

    [Fact]
    public void Hello_With_Name_Prints_Greeting_And_Exits_Zero()
    {
        var (code, stdout, stderr) = RunCli("hello", "--name", "Alice");
        code.Should().Be(0);
        stderr.Should().BeEmpty();
        stdout.Trim().Should().Be("Hello, Alice!");
    }

    [Fact]
    public void Hello_Missing_Required_Name_Fails_With_Error()
    {
        var (code, _, stderr) = RunCli("hello");
        code.Should().NotBe(0);
        stderr.Should().Contain("--name");
    }

    [Fact]
    public void Hello_Empty_Name_Fails_Validation()
    {
        // Provide whitespace to trigger non-empty validator in a cross-platform friendly way
        var (code, _, stderr) = RunCli("hello", "--name", "   ");
        code.Should().NotBe(0);
        stderr.Should().Contain("--name");
    }

    [Fact]
    public void Hello_Help_Shows_Command_Specific_Usage()
    {
        var (code, stdout, _) = RunCli("hello", "--help");
        code.Should().Be(0);
        stdout.Should().Contain("Usage:");
        stdout.Should().Contain("hello");
        stdout.Should().Contain("--name");
    }

    [Fact]
    public void Unknown_Command_Should_Error()
    {
        var (code, _, stderr) = RunCli("does-not-exist");
        code.Should().NotBe(0);
        stderr.ToLowerInvariant().Should().Contain("unrecognized");
    }
}
