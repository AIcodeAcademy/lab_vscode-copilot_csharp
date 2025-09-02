using ArchetypeCSharpCLI.Domain;
using ArchetypeCSharpCLI.Dtos;

namespace ArchetypeCSharpCLI.Mappers;

/// <summary>
/// Interface for mapping OpenMeteoResponse to WeatherReport domain model.
/// </summary>
public interface IOpenMeteoMapper
{
  /// <summary>
  /// Maps an OpenMeteoResponse to a WeatherReport domain model.
  /// </summary>
  /// <param name="response">The API response to map.</param>
  /// <param name="units">The desired unit system (metric or imperial).</param>
  /// <returns>A WeatherReport domain model.</returns>
  /// <exception cref="InvalidOperationException">Thrown when required fields are missing or invalid.</exception>
  WeatherReport MapToWeatherReport(OpenMeteoResponse response, string units);
}

/// <summary>
/// Default implementation of Open-Meteo response mapper.
/// </summary>
public class OpenMeteoMapper : IOpenMeteoMapper
{
  /// <summary>
  /// Maps an OpenMeteoResponse to a WeatherReport domain model.
  /// </summary>
  public WeatherReport MapToWeatherReport(OpenMeteoResponse response, string units)
  {
    if (response == null)
      throw new ArgumentNullException(nameof(response));

    if (response.CurrentWeather == null)
      throw new InvalidOperationException("Current weather data is required but was not provided by Open-Meteo.");

    var temp = response.CurrentWeather.Temperature;
    var code = response.CurrentWeather.Weathercode;
    var time = response.CurrentWeather.Time;
    var windSpeed = response.CurrentWeather.Windspeed;
    var windDirection = (int)response.CurrentWeather.Winddirection;
    // Humidity not present in OpenMeteoResponse, set to -1 (unknown)
    int humidity = -1;

    if (double.IsNaN(temp))
      throw new InvalidOperationException("Temperature is required but was not provided by Open-Meteo.");
    if (string.IsNullOrEmpty(time))
      throw new InvalidOperationException("Observation time is required but was not provided by Open-Meteo.");

    if (!DateTime.TryParse(time, out var observedAt))
      throw new InvalidOperationException($"Invalid observation time format: {time}");

    var temperature = (decimal)temp;
    var actualUnits = units;
    var windSpeedValue = (decimal)windSpeed;
    if (units.Equals("imperial", StringComparison.OrdinalIgnoreCase))
    {
      temperature = CelsiusToFahrenheit(temperature);
      windSpeedValue = windSpeedValue * 0.621371m; // km/h to mph
    }
    else
    {
      actualUnits = "metric";
    }

    var condition = WeatherCodeMapper.GetWeatherDescription(code);

    return new WeatherReport(
        temperature: Math.Round(temperature, 1),
        units: actualUnits,
        condition: condition,
        observedAt: observedAt,
        source: "open-meteo",
        windSpeed: Math.Round(windSpeedValue, 1),
        windDirection: windDirection,
        humidity: humidity
    );
  }

  /// <summary>
  /// Converts Celsius to Fahrenheit.
  /// </summary>
  private static decimal CelsiusToFahrenheit(decimal celsius)
  {
    return (celsius * 9 / 5) + 32;
  }
}
