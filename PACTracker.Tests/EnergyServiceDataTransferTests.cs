using System.Text.Json;
using NSubstitute;
using PACTracker.Core.Energy.DTOs;
using PACTracker.Core.Energy.Services;
using PACTracker.Domain.Entities;
using PACTracker.Infrastructure.Repositories;

namespace PACTracker.Tests;

[TestClass]
public sealed class EnergyServiceDataTransferTests
{
    [TestMethod]
    public async Task ExportDataAsync_WritesVersionedOrderedRecords()
    {
        var repository = Substitute.For<IEnergyRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Reading(new DateTime(2026, 3, 20), 20, 40),
                Reading(new DateTime(2026, 3, 10), 10, 30)
            });
        var service = new EnergyService(repository);

        var json = await service.ExportDataAsync();
        var dataFile = JsonSerializer.Deserialize<EnergyDataFileDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(dataFile);
        Assert.AreEqual(1, dataFile.Version);
        Assert.IsNotNull(dataFile.Records);
        CollectionAssert.AreEqual(
            new[] { new DateTime(2026, 3, 10), new DateTime(2026, 3, 20) },
            dataFile.Records.Select(record => record.Date).ToArray());
    }

    [TestMethod]
    public async Task ImportDataAsync_UpsertsAllValidRecords()
    {
        var repository = Substitute.For<IEnergyRepository>();
        var service = new EnergyService(repository);
        var json = JsonSerializer.Serialize(new EnergyDataFileDto
        {
            Version = 1,
            Records =
            [
                new EnergyDataRecordDto(new DateTime(2026, 3, 20), 20, 40),
                new EnergyDataRecordDto(new DateTime(2026, 3, 21), 30, 60)
            ]
        });

        var importedCount = await service.ImportDataAsync(json);

        Assert.AreEqual(2, importedCount);
        await repository.Received(1).UpsertManyAsync(
            Arg.Is<IReadOnlyCollection<EnergyEntity>>(records =>
                records.Count == 2 &&
                records.Any(record =>
                    record.Date == new DateTime(2026, 3, 20) &&
                    record.Consumption == 20 &&
                    record.Production == 40)),
            Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ImportDataAsync_RejectsDuplicateDatesWithoutWriting()
    {
        var repository = Substitute.For<IEnergyRepository>();
        var service = new EnergyService(repository);
        var date = new DateTime(2026, 3, 20);
        var json = JsonSerializer.Serialize(new EnergyDataFileDto
        {
            Version = 1,
            Records =
            [
                new EnergyDataRecordDto(date, 20, 40),
                new EnergyDataRecordDto(date.AddHours(8), 30, 60)
            ]
        });

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => service.ImportDataAsync(json));

        await repository.DidNotReceive().UpsertManyAsync(
            Arg.Any<IReadOnlyCollection<EnergyEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ImportDataAsync_RejectsFutureDatesWithoutWriting()
    {
        var repository = Substitute.For<IEnergyRepository>();
        var service = new EnergyService(repository);
        var json = JsonSerializer.Serialize(new EnergyDataFileDto
        {
            Version = 1,
            Records =
            [
                new EnergyDataRecordDto(DateTime.Today.AddDays(1), 20, 40)
            ]
        });

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => service.ImportDataAsync(json));

        await repository.DidNotReceive().UpsertManyAsync(
            Arg.Any<IReadOnlyCollection<EnergyEntity>>(),
            Arg.Any<CancellationToken>());
    }

    private static EnergyEntity Reading(DateTime date, double consumption, double production)
        => new() { Date = date, Consumption = consumption, Production = production };
}
