namespace ArchetypeCSharpCLI.Dtos.GeoIp;

public class IpApiResponseDto
{
  public string? Status { get; set; }
  public string? Message { get; set; }
  public decimal? Lat { get; set; }
  public decimal? Lon { get; set; }
  public string? City { get; set; }
  public string? RegionName { get; set; }
  public string? Country { get; set; }
}
