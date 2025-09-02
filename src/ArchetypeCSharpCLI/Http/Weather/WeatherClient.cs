using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Dtos;
using ArchetypeCSharpCLI.Domain;
using ArchetypeCSharpCLI.Mappers;
using System.Globalization;

namespace ArchetypeCSharpCLI.Http.Weather;

public class WeatherClient : IWeatherClient
{
  private readonly HttpClient _http;

  private readonly IOpenMeteoMapper _mapper;

  public WeatherClient(HttpClient http, IOpenMeteoMapper mapper)
  {
    _http = http ?? throw new ArgumentNullException(nameof(http));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  public async Task<WeatherResult> GetCurrentAsync(decimal latitude, decimal longitude, string units = "metric", bool raw = false, CancellationToken ct = default)
  {
    try
    {
      var url = $"/v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&current_weather=true";
      if (raw)
      {
        var text = await _http.GetStringAsync(url, ct).ConfigureAwait(false);
        // Try to parse for mapping but return raw regardless
        try
        {
          var parsed = System.Text.Json.JsonSerializer.Deserialize<OpenMeteoResponse>(text);
          var report = _mapper.MapToWeatherReport(parsed!, units);
          return WeatherResult.Success(report, text);
        }
        catch (Exception ex)
        {
          return WeatherResult.Failure($"Failed to parse provider response: {ex.Message}");
        }
      }

      var dto = await _http.GetFromJsonAsync<OpenMeteoResponse>(url, ct).ConfigureAwait(false);
      if (dto == null)
        return WeatherResult.Failure("Provider did not return data.");

      try
      {
        var report = _mapper.MapToWeatherReport(dto, units);
        return WeatherResult.Success(report);
      }
      catch (Exception ex)
      {
        return WeatherResult.Failure($"Mapping error: {ex.Message}");
      }
    }
    catch (OperationCanceledException)
    {
      return WeatherResult.Failure("Operation cancelled or timed out.");
    }
    catch (Exception ex)
    {
      return WeatherResult.Failure($"Network or provider error: {ex.Message}");
    }
  }
}
