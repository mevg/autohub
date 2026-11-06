using AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;
using FluentAssertions;

namespace AutoHub.Application.UnitTests.Vehicles;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator = new();

    private static CreateVehicleCommand Valid() => new(
        "1HGCM82633A123456", "Toyota", "Corolla", 2022, 250000m, 15000,
        "Automática", "Gasolina", "Seminuevo", Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("SHORT")]
    [InlineData("1HGCM82633A12345I")]
    [InlineData("1HGCM82633A12345O")]
    [InlineData("1HGCM82633A12345Q")]
    [InlineData("1hgcm82633a123456")]
    public void Invalid_vin_fails(string vin)
    {
        var result = _validator.Validate(Valid() with { Vin = vin });

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(3000)]
    public void Invalid_year_fails(int year)
    {
        _validator.Validate(Valid() with { Year = year }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Non_positive_price_fails(decimal price)
    {
        _validator.Validate(Valid() with { Price = price }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_mileage_fails()
    {
        _validator.Validate(Valid() with { Mileage = -1 }).IsValid.Should().BeFalse();
    }
}
