using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Dtos.GeoIp;
using ArchetypeCSharpCLI.Domain;

namespace ArchetypeCSharpCLI.Http.GeoIp;

public class GeoIpClient : IGeoIpClient
{
  private readonly HttpClient _http;

  public GeoIpClient(HttpClient http)
  {
    _http = http ?? throw new ArgumentNullException(nameof(http));
  }

  public async Task<GeoIpResult> GetLocationAsync(CancellationToken ct = default)
  {
    try
    {
      var dto = await _http.GetFromJsonAsync<IpApiResponseDto>("/json", ct).ConfigureAwait(false);
      if (dto == null)
        return GeoIpResult.Failure("Empty response from geoip provider.");

      if (!string.Equals(dto.Status, "success", StringComparison.OrdinalIgnoreCase))
      {
        var msg = dto.Message ?? "Geoip provider returned failure.";
        return GeoIpResult.Failure(msg);
      }

      if (!dto.Lat.HasValue || !dto.Lon.HasValue)
        return GeoIpResult.Failure("Geoip provider did not return coordinates.");

      var loc = new Location(dto.Lat.Value, dto.Lon.Value, dto.City, dto.Country);
      return GeoIpResult.Success(loc);
    }
    catch (OperationCanceledException)
    {
      return GeoIpResult.Failure("Operation cancelled or timed out.");
    }
    catch (Exception ex)
    {
      return GeoIpResult.Failure($"Network or provider error: {ex.Message}");
    }
  }
}
