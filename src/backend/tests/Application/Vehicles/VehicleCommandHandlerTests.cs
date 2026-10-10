using AutoHub.Application.Common.Exceptions;
using AutoHub.Application.Common.Interfaces;
using AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;
using AutoHub.Application.Features.Vehicles.Commands.UpdateVehicle;
using AutoHub.Domain.Entities;
using AutoHub.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace AutoHub.Application.UnitTests.Vehicles;

public class VehicleCommandHandlerTests
{
    private readonly IVehicleStore _store = Substitute.For<IVehicleStore>();
    private readonly IVehicleQueries _queries = Substitute.For<IVehicleQueries>();

    private static CreateVehicleCommand CreateCmd() => new(
        "1HGCM82633A123456", "Toyota", "Corolla", 2022, 250000m, 15000,
        "Automática", "Gasolina", null, Guid.NewGuid());

    private static Vehicle ExistingVehicle() => new()
    {
        Id = Guid.NewGuid(),
        Vin = "1HGCM82633A123456",
        Make = "Toyota",
        Model = "Corolla",
        Year = 2022,
        Price = 250000m,
        Mileage = 15000,
        Transmission = "Automática",
        FuelType = "Gasolina",
        Status = VehicleStatus.Available,
        CreatedBy = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_with_new_vin_adds_vehicle()
    {
        _queries.VinExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        Vehicle? added = null;
        _store.Add(Arg.Do<Vehicle>(v => added = v));

        var id = await new CreateVehicleCommandHandler(_store, _queries)
            .Handle(CreateCmd(), CancellationToken.None);

        added.Should().NotBeNull();
        added!.Id.Should().Be(id);
        added.Vin.Should().Be("1HGCM82633A123456");
        added.Status.Should().Be(VehicleStatus.Available);
        await _store.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_with_duplicate_vin_throws()
    {
        _queries.VinExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => new CreateVehicleCommandHandler(_store, _queries)
            .Handle(CreateCmd(), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateVinException>();
        _store.DidNotReceive().Add(Arg.Any<Vehicle>());
    }

    [Fact]
    public async Task Update_missing_vehicle_throws_not_found()
    {
        _store.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var act = () => new UpdateVehicleCommandHandler(_store, _queries).Handle(
            new UpdateVehicleCommand(Guid.NewGuid(), "1HGCM82633A123456", "Toyota", "Corolla",
                2022, 250000m, 15000, "Automática", "Gasolina", "Available", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_archived_vehicle_throws()
    {
        var vehicle = ExistingVehicle();
        vehicle.Status = VehicleStatus.Archived;
        _store.GetForUpdateAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var act = () => new UpdateVehicleCommandHandler(_store, _queries).Handle(
            new UpdateVehicleCommand(vehicle.Id, vehicle.Vin, "Toyota", "Corolla",
                2022, 250000m, 15000, "Automática", "Gasolina", "Available", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Update_applies_changes_and_status()
    {
        var vehicle = ExistingVehicle();
        _store.GetForUpdateAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        await new UpdateVehicleCommandHandler(_store, _queries).Handle(
            new UpdateVehicleCommand(vehicle.Id, vehicle.Vin, "Honda", "Civic",
                2023, 300000m, 5000, "Manual", "Gasolina", "Reserved", "Apartado"),
            CancellationToken.None);

        vehicle.Make.Should().Be("Honda");
        vehicle.Status.Should().Be(VehicleStatus.Reserved);
        vehicle.Description.Should().Be("Apartado");
        vehicle.UpdatedAt.Should().NotBeNull();
    }
}
