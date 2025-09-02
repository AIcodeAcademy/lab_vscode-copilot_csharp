using System.Text.Json.Serialization;

namespace ArchetypeCSharpCLI.Dtos;

/// <summary>
/// DTO for ip-api.com geolocation API response.
/// Provides minimal fields needed for weather functionality.
/// </summary>
public class IpApiResponse
{
  /// <summary>
  /// Response status (success/fail).
  /// </summary>
  [JsonPropertyName("status")]
  public string? Status { get; set; }

  /// <summary>
  /// Latitude in decimal degrees.
  /// </summary>
  [JsonPropertyName("lat")]
  public decimal? Lat { get; set; }

  /// <summary>
  /// Longitude in decimal degrees.
  /// </summary>
  [JsonPropertyName("lon")]
  public decimal? Lon { get; set; }

  /// <summary>
  /// City name.
  /// </summary>
  [JsonPropertyName("city")]
  public string? City { get; set; }

  /// <summary>
  /// Country name.
  /// </summary>
  [JsonPropertyName("country")]
  public string? Country { get; set; }
}
