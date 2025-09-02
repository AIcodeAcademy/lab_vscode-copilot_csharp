using ArchetypeCSharpCLI.Configuration;
using Microsoft.Extensions.Logging;

namespace ArchetypeCSharpCLI.Logging;

/// <summary>
/// Centralized logger factory and helpers for creating loggers.
/// Configured from <see cref="AppSettings"/> (LogLevel) and writes to console.
/// </summary>
public static class Log
{
  private static ILoggerFactory? _factory;
  private static readonly object _lock = new();

  private static ILoggerFactory Factory
  {
    get
    {
      if (_factory is not null) return _factory;
      lock (_lock)
      {
        _factory ??= CreateFactory();
      }
      return _factory;
    }
  }

  private static ILoggerFactory CreateFactory()
  {
    var level = ParseLevel(AppSettings.Current.LogLevel);
    if (Environment.GetEnvironmentVariable("AIDDBOT_VERBOSE") == "1")
    {
      level = LogLevel.Debug;
    }
    return LoggerFactory.Create(builder =>
    {
      builder.SetMinimumLevel(level);
      builder.AddSimpleConsole(options =>
          {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
            options.IncludeScopes = true;
          });
    });
  }

  /// <summary>
  /// Parses a string into a LogLevel, defaulting to Information on invalid input.
  /// </summary>
  private static LogLevel ParseLevel(string? value)
  {
    return Enum.TryParse<LogLevel>(value, true, out var lvl) ? lvl : LogLevel.Information;
  }

  public static ILogger<T> For<T>() => Factory.CreateLogger<T>();
  public static ILogger For(Type type) => Factory.CreateLogger(type.FullName ?? type.Name);
  public static ILogger For(string category) => Factory.CreateLogger(category);
}
