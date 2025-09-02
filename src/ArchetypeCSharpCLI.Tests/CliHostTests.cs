using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class CliHostTests
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
        var dir = Path.GetDirectoryName(typeof(CliHostTests).Assembly.Location)!;
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
    public void Help_Shows_Usage_And_Options()
    {
        var (code, stdout, _) = RunCli("--help");
        code.Should().Be(0);
        stdout.Should().Contain("Usage:");
        stdout.Should().Contain("archetype");
        stdout.Should().Contain("--version");
    }

    [Fact]
    public void NoArgs_Shows_Help()
    {
        var (code, stdout, _) = RunCli();
        code.Should().Be(0);
        stdout.Should().Contain("Usage:");
    }

    [Theory]
    [InlineData("--version")]
    [InlineData("-v")]
    public void Version_Prints_SemVer_And_Exits_Zero(string flag)
    {
        var (code, stdout, _) = RunCli(flag);
        code.Should().Be(0);
        stdout.Trim().Should().MatchRegex(@"^\d+\.\d+\.\d+(?:[-+].*)?$");
    }

    [Fact]
    public void Help_Takes_Precendence_Over_Version()
    {
        var (code, stdout, _) = RunCli("--help", "--version");
        code.Should().Be(0);
        stdout.Should().Contain("Usage:");
    }
}
