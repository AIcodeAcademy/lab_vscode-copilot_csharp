using System;
using System.IO;
using ArchetypeCSharpCLI.Configuration;
using FluentAssertions;
using ArchetypeCSharpCLI.Tests.TestUtils;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

[Collection("ConfigFiles")]
public class ConfigPipelineTests
{
  [Fact]
  public void Defaults_Are_Applied_When_No_Files_Or_Env()
  {
    using var _env = new EnvVarScope(("DOTNET_ENVIRONMENT", null), ("ASPNETCORE_ENVIRONMENT", null),
                                     ("App__Environment", null), ("App__HttpTimeoutSeconds", null), ("App__LogLevel", null),
                                     ("Environment", null), ("HttpTimeoutSeconds", null), ("LogLevel", null));
    using var _files = TempSettingsFiles.Create();

    var cfg = ConfigBuilder.Build();

    cfg.Environment.Should().Be("Production");
    cfg.HttpTimeoutSeconds.Should().Be(30);
    cfg.LogLevel.Should().Be("Information");
  }

  [Fact]
  public void Appsettings_File_Values_Are_Read()
  {
    using var _env = new EnvVarScope(("DOTNET_ENVIRONMENT", null), ("ASPNETCORE_ENVIRONMENT", null));
    using var _files = TempSettingsFiles.Create(new { Environment = "Development", HttpTimeoutSeconds = 10, LogLevel = "Debug" });

    var cfg = ConfigBuilder.Build();

    cfg.Environment.Should().Be("Development");
    cfg.HttpTimeoutSeconds.Should().Be(10);
    cfg.LogLevel.Should().Be("Debug");
  }

  [Fact]
  public void Environment_Specific_File_Overrides_Base()
  {
    using var _env = new EnvVarScope(("DOTNET_ENVIRONMENT", "Development"), ("ASPNETCORE_ENVIRONMENT", null));
    using var _files = TempSettingsFiles.Create(appsettings: new { HttpTimeoutSeconds = 10 }, appsettingsEnv: new { HttpTimeoutSeconds = 25 }, envName: "Development");

    var cfg = ConfigBuilder.Build();

    cfg.HttpTimeoutSeconds.Should().Be(25);
  }

  [Fact]
  public void Env_Variables_Override_Files_With_Prefix()
  {
    using var _env = new EnvVarScope(("DOTNET_ENVIRONMENT", "Development"), ("App__HttpTimeoutSeconds", "45"));
    using var _files = TempSettingsFiles.Create(appsettings: new { HttpTimeoutSeconds = 10 }, appsettingsEnv: new { HttpTimeoutSeconds = 25 }, envName: "Development");

    var cfg = ConfigBuilder.Build();
    cfg.HttpTimeoutSeconds.Should().Be(45);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-5)]
  [InlineData(999)]
  public void Invalid_HttpTimeout_Is_Clamped_To_Default(int invalid)
  {
    using var _env = new EnvVarScope(("DOTNET_ENVIRONMENT", null));
    using var _files = TempSettingsFiles.Create(new { HttpTimeoutSeconds = invalid });

    var cfg = ConfigBuilder.Build();
    cfg.HttpTimeoutSeconds.Should().Be(30);
  }
}
