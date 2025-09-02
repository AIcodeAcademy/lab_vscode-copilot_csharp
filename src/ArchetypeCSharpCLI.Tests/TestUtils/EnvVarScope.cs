using System;

namespace ArchetypeCSharpCLI.Tests.TestUtils;

/// <summary>
/// Temporarily sets environment variables and restores previous values on dispose.
/// Pass null value to clear a variable during the scope.
/// </summary>
public sealed class EnvVarScope : IDisposable
{
  private readonly (string key, string? prev)[] _pairs;

  public EnvVarScope(params (string key, string? value)[] vars)
  {
    _pairs = new (string, string?)[vars.Length];
    for (int i = 0; i < vars.Length; i++)
    {
      var (k, v) = vars[i];
      _pairs[i] = (k, Environment.GetEnvironmentVariable(k));
      Environment.SetEnvironmentVariable(k, v);
    }
  }

  public void Dispose()
  {
    foreach (var (k, prev) in _pairs)
    {
      Environment.SetEnvironmentVariable(k, prev);
    }
  }
}
