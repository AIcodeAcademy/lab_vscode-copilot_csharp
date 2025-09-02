using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Domain;

namespace ArchetypeCSharpCLI.Http.Weather;

public interface IWeatherClient
{
  Task<WeatherResult> GetCurrentAsync(decimal latitude, decimal longitude, string units = "metric", bool raw = false, CancellationToken ct = default);
}

public sealed class WeatherResult
{
  public bool IsSuccess { get; }
  public WeatherReport? Report { get; }
  public string? RawJson { get; }
  public string? ErrorMessage { get; }

  private WeatherResult(bool success, WeatherReport? report, string? rawJson, string? error)
  {
    IsSuccess = success;
    Report = report;
    RawJson = rawJson;
    ErrorMessage = error;
  }

  public static WeatherResult Success(WeatherReport r, string? rawJson = null) => new(true, r, rawJson, null);
  public static WeatherResult Failure(string message) => new(false, null, null, message);
}
