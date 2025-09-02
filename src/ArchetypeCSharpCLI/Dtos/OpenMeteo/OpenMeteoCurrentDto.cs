namespace ArchetypeCSharpCLI.Dtos.OpenMeteo;

public class OpenMeteoCurrentDto
{
  public CurrentWeatherDto? current_weather { get; set; }

  public class CurrentWeatherDto
  {
    public decimal? temperature { get; set; }
    public decimal? windspeed { get; set; }
    public int? weathercode { get; set; }
  }
}
