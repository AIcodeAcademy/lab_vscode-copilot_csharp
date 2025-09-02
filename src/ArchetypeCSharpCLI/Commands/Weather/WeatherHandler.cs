using System;
using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Http.GeoIp;
using ArchetypeCSharpCLI.Http.Weather;
using ArchetypeCSharpCLI.Domain;
using ArchetypeCSharpCLI.Configuration.Binding;

namespace ArchetypeCSharpCLI.Commands.Weather;

/// <summary>
/// Handles the 'weather' command execution. Resolves coordinates (GeoIP or provided),
/// calls the weather client, and prints a human-friendly report including emojis,
/// temperature, wind speed/direction, humidity (when available), condition, and timestamps.
/// Respects the --units and --raw options.
/// </summary>
public static class WeatherHandler
{
  /// <summary>
  /// Executes the weather command.
  /// </summary>
  /// <param name="lat">Optional latitude in decimal degrees.</param>
  /// <param name="lon">Optional longitude in decimal degrees.</param>
  /// <param name="timeoutSeconds">Optional timeout in seconds for the operation.</param>
  /// <param name="units">Unit system (metric or imperial). Defaults to metric.</param>
  /// <param name="raw">When true, prints the raw provider JSON instead of formatted output.</param>
  /// <returns>Process exit code.</returns>
  public static async Task<int> HandleAsync(decimal? lat, decimal? lon, int? timeoutSeconds, string units = "metric", bool raw = false)
  {
    // Resolve services from a simple service locator available in OptionsBootstrap
    var provider = OptionsBootstrap.Services ?? throw new InvalidOperationException("DI services not initialized");

    var geo = provider.GetService(typeof(IGeoIpClient)) as IGeoIpClient;
    var weather = provider.GetService(typeof(IWeatherClient)) as IWeatherClient;

    if (weather == null)
    {
      Console.Error.WriteLine("Weather client is not registered.");
      return ExitCodes.Unexpected;
    }

    decimal latitude, longitude;

    if (lat.HasValue && lon.HasValue)
    {
      latitude = lat.Value;
      longitude = lon.Value;
    }
    else
    {
      if (geo == null)
      {
        Console.Error.WriteLine("GeoIP client not available to resolve coordinates.");
        return ExitCodes.Unexpected;
      }

      var geoResult = await geo.GetLocationAsync();
      if (!geoResult.IsSuccess)
      {
        Console.Error.WriteLine(geoResult.ErrorMessage);
        return ExitCodes.NetworkOrTimeout;
      }

      var loc = geoResult.Location!;
      latitude = loc.Latitude;
      longitude = loc.Longitude;
    }

    using var cts = timeoutSeconds.HasValue ? new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds.Value)) : new CancellationTokenSource();

    var weatherResult = await weather.GetCurrentAsync(latitude, longitude, units, raw, cts.Token);
    if (!weatherResult.IsSuccess)
    {
      Console.Error.WriteLine(weatherResult.ErrorMessage);
      return ExitCodes.NetworkOrTimeout;
    }
    if (raw && !string.IsNullOrEmpty(weatherResult.RawJson))
    {
      Console.WriteLine(weatherResult.RawJson);
      return ExitCodes.Success;
    }

    var report = weatherResult.Report!;
    // Write enhanced human-friendly output with emojis and details
    string tempEmoji = report.Temperature > 30 ? "🌡️" : report.Temperature < 10 ? "❄️" : "🌤️";
    string windEmoji = "💨";
    string humidityEmoji = "💧";
    string conditionEmoji = GetConditionEmoji(report.Condition);
    string windDir = GetWindDirection(report.WindDirection);
    string humidityStr = report.Humidity >= 0 ? $"{humidityEmoji} Humidity: {report.Humidity}%" : "";

    Console.WriteLine($"{conditionEmoji} Weather Report");
    Console.WriteLine($"{tempEmoji} Temperature: {report.Temperature}°{(report.Units == "metric" ? "C" : "F")}");
    Console.WriteLine($"{windEmoji} Wind: {report.WindSpeed} {(report.Units == "metric" ? "km/h" : "mph")} {windDir}");
    if (!string.IsNullOrEmpty(humidityStr)) Console.WriteLine(humidityStr);
    Console.WriteLine($"Observed at: {report.ObservedAt:u}");
    Console.WriteLine($"Condition: {report.Condition}");
    Console.WriteLine($"Source: {report.Source}");
    return ExitCodes.Success;

    // Local helpers
    static string GetConditionEmoji(string condition)
    {
      if (condition.Contains("rain", StringComparison.OrdinalIgnoreCase)) return "🌧️";
      if (condition.Contains("cloud", StringComparison.OrdinalIgnoreCase)) return "☁️";
      if (condition.Contains("clear", StringComparison.OrdinalIgnoreCase)) return "☀️";
      if (condition.Contains("snow", StringComparison.OrdinalIgnoreCase)) return "❄️";
      if (condition.Contains("storm", StringComparison.OrdinalIgnoreCase)) return "⛈️";
      return "🌤️";
    }
    static string GetWindDirection(int degrees)
    {
      string[] dirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
      int idx = (int)Math.Round(((double)degrees % 360) / 45) % 8;
      return dirs[idx];
    }
  }
}
