namespace ArchetypeCSharpCLI.Domain;

/// <summary>
/// Represents a geographical location with coordinates and place names.
/// </summary>
public class Location
{
  /// <summary>
  /// Latitude in decimal degrees (-90 to 90).
  /// </summary>
  public decimal Latitude { get; }

  /// <summary>
  /// Longitude in decimal degrees (-180 to 180).
  /// </summary>
  public decimal Longitude { get; }

  /// <summary>
  /// City name, if available.
  /// </summary>
  public string? City { get; }

  /// <summary>
  /// Country name, if available.
  /// </summary>
  public string? Country { get; }

  /// <summary>
  /// Initializes a new instance of the Location class.
  /// </summary>
  public Location(decimal latitude, decimal longitude, string? city = null, string? country = null)
  {
    if (latitude < -90 || latitude > 90)
      throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

    if (longitude < -180 || longitude > 180)
      throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

    Latitude = latitude;
    Longitude = longitude;
    City = city;
    Country = country;
  }
}
