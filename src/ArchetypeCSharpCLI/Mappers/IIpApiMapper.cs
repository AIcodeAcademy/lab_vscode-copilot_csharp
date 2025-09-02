using ArchetypeCSharpCLI.Domain;
using ArchetypeCSharpCLI.Dtos;

namespace ArchetypeCSharpCLI.Mappers;

/// <summary>
/// Interface for mapping IpApiResponse to Location domain model.
/// </summary>
public interface IIpApiMapper
{
  /// <summary>
  /// Maps an IpApiResponse to a Location domain model.
  /// </summary>
  /// <param name="response">The API response to map.</param>
  /// <returns>A Location domain model.</returns>
  /// <exception cref="InvalidOperationException">Thrown when required fields are missing or invalid.</exception>
  Location MapToLocation(IpApiResponse response);
}

/// <summary>
/// Default implementation of IP-API response mapper.
/// </summary>
public class IpApiMapper : IIpApiMapper
{
  /// <summary>
  /// Maps an IpApiResponse to a Location domain model.
  /// </summary>
  public Location MapToLocation(IpApiResponse response)
  {
    if (response == null)
      throw new ArgumentNullException(nameof(response));

    if (string.IsNullOrEmpty(response.Status) || !response.Status.Equals("success", StringComparison.OrdinalIgnoreCase))
      throw new InvalidOperationException("IP-API request was not successful.");

    if (!response.Lat.HasValue || !response.Lon.HasValue)
      throw new InvalidOperationException("Latitude and longitude are required but were not provided by IP-API.");

    return new Location(
        latitude: response.Lat.Value,
        longitude: response.Lon.Value,
        city: response.City,
        country: response.Country
    );
  }
}
