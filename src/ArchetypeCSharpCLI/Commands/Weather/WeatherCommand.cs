using System.CommandLine;
using System.Globalization;
using ArchetypeCSharpCLI.Http.GeoIp;
using ArchetypeCSharpCLI.Http.Weather;

namespace ArchetypeCSharpCLI.Commands.Weather;

public static class WeatherCommand
{
  public static Command Build()
  {
    var cmd = new Command("weather", "Show current weather for the current IP or provided coordinates");

    var lat = new Option<string?>("--lat", "Latitude in decimal degrees (use dot as decimal separator, e.g., 40.4168)");
    var lon = new Option<string?>("--lon", "Longitude in decimal degrees (use dot as decimal separator, e.g., -3.7038)");
    var timeout = new Option<int?>("--timeout", () => null, "Timeout in seconds for the operation");
    var units = new Option<string>(new[] { "--units" }, () => "metric", "Units to display: metric or imperial");
    var raw = new Option<bool>("--raw", "Print raw JSON from provider");

    cmd.AddOption(lat);
    cmd.AddOption(lon);
    cmd.AddOption(timeout);
    cmd.AddOption(units);
    cmd.AddOption(raw);

    cmd.SetHandler(async (string? latStr, string? lonStr, int? timeoutVal, string unitsVal, bool rawVal) =>
    {
      decimal? latVal = ParseDecimalInvariant(latStr);
      decimal? lonVal = ParseDecimalInvariant(lonStr);
      var opts = new { Latitude = latVal, Longitude = lonVal, TimeoutSeconds = timeoutVal, Units = unitsVal };
      var code = await WeatherHandler.HandleAsync(opts.Latitude, opts.Longitude, opts.TimeoutSeconds, opts.Units, rawVal);
      if (code != 0)
      {
        Console.Error.WriteLine($"weather command failed with exit code {code}");
        Environment.ExitCode = code;
      }
    }, lat, lon, timeout, units, raw);

    return cmd;
  }

  private static decimal? ParseDecimalInvariant(string? input)
  {
    if (string.IsNullOrWhiteSpace(input)) return null;
    if (decimal.TryParse(input, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var v))
    {
      return v;
    }
    // Fallback to current culture to be user-friendly
    if (decimal.TryParse(input, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out var v2))
    {
      return v2;
    }
    Console.Error.WriteLine($"Invalid coordinate value: '{input}'. Expected a decimal number like 40.4168.");
    return null;
  }
}
