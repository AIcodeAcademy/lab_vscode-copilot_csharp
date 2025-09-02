using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using ArchetypeCSharpCLI.Configuration.Binding;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ArchetypeCSharpCLI.Tests.TestUtils;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

[Collection("ConfigFiles")]
public class ConfigBindingTests
{
  private sealed class SampleOptions
  {
    [Required]
    public string RequiredName { get; init; } = string.Empty;

    [Range(1, 100)]
    public int Threshold { get; init; } = 10;

    public string Mode { get; init; } = "basic";
  }

  private static IConfigurationRoot BuildConfigFromFiles()
  {
    // Mirrors ConfigBuilder.BuildRaw: base path + reloadOnChange enabled
    return new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
  }

  [Fact]
  public void Binds_From_Section_And_Preserves_Defaults()
  {
    using var _files = TempSettingsFiles.Create(new
    {
      Sample = new { RequiredName = "alpha", Threshold = 30 }
    });

    var configuration = BuildConfigFromFiles();
    var sp = OptionsBootstrap.Init(configuration, services =>
    {
      services.AddBoundOptions<SampleOptions>(configuration, sectionName: "Sample");
    });

    var options = sp.GetRequiredService<IOptions<SampleOptions>>().Value;

    options.RequiredName.Should().Be("alpha");
    options.Threshold.Should().Be(30);
    // Not provided in JSON -> keep default
    options.Mode.Should().Be("basic");
  }

  [Fact]
  public void Missing_Required_Property_Fails_Validation()
  {
    using var _files = TempSettingsFiles.Create(new
    {
      Sample = new { Threshold = 10 } // RequiredName missing
    });

    var configuration = BuildConfigFromFiles();
    var sp = OptionsBootstrap.Init(configuration, services =>
    {
      services.AddBoundOptions<SampleOptions>(configuration, sectionName: "Sample");
    });

    var act = () => sp.GetRequiredService<IOptions<SampleOptions>>().Value.RequiredName.Length.Should().BeGreaterThan(0);
    act.Should().Throw<OptionsValidationException>()
        .WithMessage("*SampleOptions*");
  }

  [Fact]
  public void Custom_Validation_Can_Reject_Invalid_Config()
  {
    using var _files = TempSettingsFiles.Create(new
    {
      Sample = new { RequiredName = "alpha", Threshold = 5 }
    });

    var configuration = BuildConfigFromFiles();
    var sp = OptionsBootstrap.Init(configuration, services =>
    {
      services.AddBoundOptions<SampleOptions>(configuration, "Sample",
              validate: o => o.Mode is "advanced" || o.Mode is "basic" && o.Threshold >= 10,
              validateError: "Threshold must be >= 10 for basic mode");
    });

    var act = () => sp.GetRequiredService<IOptions<SampleOptions>>().Value.Threshold.Should().BeGreaterOrEqualTo(10);
    act.Should().Throw<OptionsValidationException>()
       .WithMessage("*Threshold must be >= 10 for basic mode*");
  }

  [Fact]
  public void Environment_Vars_Override_File()
  {
    using var _env = new EnvVarScope(("Sample__Threshold", "42"));
    using var _files = TempSettingsFiles.Create(new { Sample = new { RequiredName = "alpha", Threshold = 10 } });

    var configuration = BuildConfigFromFiles();
    var sp = OptionsBootstrap.Init(configuration, services =>
    {
      services.AddBoundOptions<SampleOptions>(configuration, "Sample");
    });

    var value = sp.GetRequiredService<IOptions<SampleOptions>>().Value;
    value.Threshold.Should().Be(42);
  }

  [Fact]
  public void OptionsMonitor_Reflects_File_Changes()
  {
    using var _files = TempSettingsFiles.Create(new
    {
      Sample = new { RequiredName = "alpha", Threshold = 10 }
    });

    var configuration = BuildConfigFromFiles();
    var sp = OptionsBootstrap.Init(configuration, services =>
    {
      services.AddBoundOptions<SampleOptions>(configuration, "Sample");
    });

    var monitor = sp.GetRequiredService<IOptionsMonitor<SampleOptions>>();
    monitor.CurrentValue.Threshold.Should().Be(10);

    // Overwrite appsettings.json with new value and wait for reload
    TempSettingsFiles.Overwrite(new
    {
      Sample = new { RequiredName = "alpha", Threshold = 25 }
    });

    var updated = SpinWait.SpinUntil(() => monitor.CurrentValue.Threshold == 25, TimeSpan.FromSeconds(3));
    updated.Should().BeTrue("options monitor should observe file reload changes");
  }
}
