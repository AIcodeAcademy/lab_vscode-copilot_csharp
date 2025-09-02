using System;
using ArchetypeCSharpCLI.Domain;
using ArchetypeCSharpCLI.Dtos;
using ArchetypeCSharpCLI.Mappers;
using FluentAssertions;
using Xunit;

namespace ArchetypeCSharpCLI.Tests;

public class DtoMappingTests
{
  [Fact]
  public void IpApiMapper_MapsSuccessResponse_ToLocation()
  {
    var mapper = new IpApiMapper();
    var dto = new IpApiResponse { Status = "success", Lat = 40.4m, Lon = -3.7m, City = "City", Country = "Country" };
    var loc = mapper.MapToLocation(dto);
    loc.Latitude.Should().Be(40.4m);
    loc.Longitude.Should().Be(-3.7m);
    loc.City.Should().Be("City");
    loc.Country.Should().Be("Country");
  }

  [Fact]
  public void IpApiMapper_MissingLatLon_Throws()
  {
    var mapper = new IpApiMapper();
    var dto = new IpApiResponse { Status = "success" };
    Action act = () => mapper.MapToLocation(dto);
    act.Should().Throw<InvalidOperationException>();
  }


  [Theory]
  [InlineData("metric", 20.0, 20.0)]
  [InlineData("imperial", 25.0, 77.0)]
  public void OpenMeteoMapper_MapsToWeatherReport_WithUnits(string units, double celsius, double expected)
  {
    var mapper = new OpenMeteoMapper();
    var dto = new OpenMeteoResponse
    {
      CurrentWeather = new CurrentWeather { Temperature = celsius, Weathercode = 0, Time = "2025-08-28T10:00:00Z" }
    };
    var report = mapper.MapToWeatherReport(dto, units);
    ((double)report.Temperature).Should().BeApproximately(expected, 0.05);
    report.Source.Should().Be("open-meteo");
    report.Condition.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public void OpenMeteoMapper_MissingFields_Throws()
  {
    var mapper = new OpenMeteoMapper();
    var dto = new OpenMeteoResponse { CurrentWeather = new CurrentWeather() };
    Action act = () => mapper.MapToWeatherReport(dto, "metric");
    act.Should().Throw<InvalidOperationException>();
  }
}
