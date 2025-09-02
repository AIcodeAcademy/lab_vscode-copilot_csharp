using System.Threading;
using System.Threading.Tasks;
using ArchetypeCSharpCLI.Domain;

namespace ArchetypeCSharpCLI.Http.GeoIp;

public interface IGeoIpClient
{
  Task<GeoIpResult> GetLocationAsync(CancellationToken ct = default);
}

public sealed class GeoIpResult
{
  public bool IsSuccess { get; }
  public Location? Location { get; }
  public string? ErrorMessage { get; }

  private GeoIpResult(bool success, Location? location, string? error)
  {
    IsSuccess = success;
    Location = location;
    ErrorMessage = error;
  }

  public static GeoIpResult Success(Location loc) => new(true, loc, null);
  public static GeoIpResult Failure(string message) => new(false, null, message);
}
