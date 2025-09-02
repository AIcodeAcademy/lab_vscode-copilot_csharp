namespace ArchetypeCSharpCLI.Domain;

/// <summary>
/// Represents a weather report for a specific location and time including
/// temperature, condition, wind, and optional humidity.
/// </summary>
public class WeatherReport
{
  /// <summary>
  /// Current temperature.
  /// </summary>
  public decimal Temperature { get; }

  /// <summary>
  /// Unit system for the temperature (metric or imperial).
  /// </summary>
  public string Units { get; }

  /// <summary>
  /// Human-readable weather condition description.
  /// </summary>
  public string Condition { get; }

  /// <summary>
  /// Timestamp when the weather was observed.
  /// </summary>
  public DateTime ObservedAt { get; }

  /// <summary>
  /// Source of the weather data (e.g., "open-meteo").
  /// </summary>
  public string Source { get; }

  /// <summary>
  /// Wind speed in km/h or mph.
  /// </summary>
  public decimal WindSpeed { get; }

  /// <summary>
  /// Wind direction in degrees.
  /// </summary>
  public int WindDirection { get; }

  /// <summary>
  /// Humidity percentage (0-100).
  /// </summary>
  public int Humidity { get; }

  /// <summary>
  /// Initializes a new instance of the WeatherReport class.
  /// </summary>
  /// <param name="temperature">Air temperature in degrees (Celsius or Fahrenheit depending on <paramref name="units"/>).</param>
  /// <param name="units">Unit system, either "metric" (C, km/h) or "imperial" (F, mph).</param>
  /// <param name="condition">Human-friendly weather condition description.</param>
  /// <param name="observedAt">Timestamp when the observation was measured (UTC).</param>
  /// <param name="source">Provider source name (e.g., "open-meteo").</param>
  /// <param name="windSpeed">Wind speed in km/h (metric) or mph (imperial).</param>
  /// <param name="windDirection">Wind direction in degrees.</param>
  /// <param name="humidity">Relative humidity percentage 0-100; -1 when unknown.</param>
  public WeatherReport(decimal temperature, string units, string condition, DateTime observedAt, string source, decimal windSpeed, int windDirection, int humidity)
  {
    if (string.IsNullOrWhiteSpace(units))
      throw new ArgumentException("Units cannot be null or empty.", nameof(units));

    if (string.IsNullOrWhiteSpace(condition))
      throw new ArgumentException("Condition cannot be null or empty.", nameof(condition));

    if (string.IsNullOrWhiteSpace(source))
      throw new ArgumentException("Source cannot be null or empty.", nameof(source));

    Temperature = temperature;
    Units = units;
    Condition = condition;
    ObservedAt = observedAt;
    Source = source;
    WindSpeed = windSpeed;
    WindDirection = windDirection;
    Humidity = humidity;
  }
}
