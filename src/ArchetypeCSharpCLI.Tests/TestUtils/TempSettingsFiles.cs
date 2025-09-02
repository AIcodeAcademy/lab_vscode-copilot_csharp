using System;
using System.IO;
using System.Text.Json;

namespace ArchetypeCSharpCLI.Tests.TestUtils;

/// <summary>
/// Creates temporary appsettings.json and appsettings.{env}.json in the AppContext.BaseDirectory
/// and removes them on dispose.
/// </summary>
public sealed class TempSettingsFiles : IDisposable
{
  private readonly string _baseDir = AppContext.BaseDirectory;
  private string[] _created = Array.Empty<string>();

  public static TempSettingsFiles Create(object? appsettings = null, object? appsettingsEnv = null, string? envName = null)
  {
    var tsf = new TempSettingsFiles();
    var created = new System.Collections.Generic.List<string>();
    if (appsettings is not null)
    {
      var p = Path.Combine(tsf._baseDir, "appsettings.json");
      File.WriteAllText(p, JsonSerializer.Serialize(appsettings));
      created.Add(p);
    }
    if (appsettingsEnv is not null && !string.IsNullOrWhiteSpace(envName))
    {
      var p = Path.Combine(tsf._baseDir, $"appsettings.{envName}.json");
      File.WriteAllText(p, JsonSerializer.Serialize(appsettingsEnv));
      created.Add(p);
    }
    tsf._created = created.ToArray();
    return tsf;
  }

  public static void Overwrite(object appsettings)
  {
    var p = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    File.WriteAllText(p, JsonSerializer.Serialize(appsettings));
  }

  public void Dispose()
  {
    foreach (var p in _created)
    {
      try { if (File.Exists(p)) File.Delete(p); } catch { /* ignore */ }
    }
  }
}
