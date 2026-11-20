using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PACTracker.Domain.Entities;
using PACTracker.Infrastructure.Injection;
using PACTracker.Infrastructure.Repositories;

namespace PACTracker.Tests;

[TestClass]
public sealed class EnergyRepositoryTests
{
    [TestMethod]
    public async Task UpsertAsync_InsertsAndUpdatesOneRecordPerDay()
    {
        await using var database = new DatabaseFixture();
        var date = new DateTime(2026, 3, 20);

        await database.Repository.UpsertAsync(new EnergyEntity
        {
            Date = date.AddHours(8),
            Consumption = 100,
            Production = 300
        });
        await database.Repository.UpsertAsync(new EnergyEntity
        {
            Date = date.AddHours(18),
            Consumption = 125,
            Production = 350
        });

        var records = await database.Repository.GetByMonthAsync(2026, 3);
        var record = records.Single();

        Assert.AreEqual(date, record.Date);
        Assert.AreEqual(125, record.Consumption);
        Assert.AreEqual(350, record.Production);
    }

    [TestMethod]
    public async Task GetByMonthAndYearAsync_ReturnsOnlyMatchingOrderedRecords()
    {
        await using var database = new DatabaseFixture();
        await database.Repository.UpsertAsync(Reading(new DateTime(2026, 2, 1), 20, 40));
        await database.Repository.UpsertAsync(Reading(new DateTime(2026, 1, 31), 10, 30));
        await database.Repository.UpsertAsync(Reading(new DateTime(2025, 12, 31), 5, 15));

        var january = await database.Repository.GetByMonthAsync(2026, 1);
        var year = await database.Repository.GetByYearAsync(2026);

        Assert.AreEqual(new DateTime(2026, 1, 31), january.Single().Date);
        CollectionAssert.AreEqual(
            new[] { new DateTime(2026, 1, 31), new DateTime(2026, 2, 1) },
            year.Select(record => record.Date).ToArray());
    }

    [TestMethod]
    public async Task GetByDateAsync_ReturnsNullWhenRecordDoesNotExist()
    {
        await using var database = new DatabaseFixture();

        var record = await database.Repository.GetByDateAsync(new DateTime(2026, 3, 20));

        Assert.IsNull(record);
    }

    [TestMethod]
    [DataRow(-1, 300)]
    [DataRow(100, double.NaN)]
    [DataRow(100, double.PositiveInfinity)]
    public async Task UpsertAsync_RejectsInvalidValuesWithoutChangingExistingRecord(
        double consumption,
        double production)
    {
        await using var database = new DatabaseFixture();
        var date = new DateTime(2026, 3, 20);
        await database.Repository.UpsertAsync(Reading(date, 100, 300));

        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            database.Repository.UpsertAsync(Reading(date, consumption, production)));

        var record = await database.Repository.GetByDateAsync(date);
        Assert.IsNotNull(record);
        Assert.AreEqual(100, record.Consumption);
        Assert.AreEqual(300, record.Production);
    }

    private static EnergyEntity Reading(DateTime date, double consumption, double production)
        => new() { Date = date, Consumption = consumption, Production = production };

    private sealed class DatabaseFixture : IAsyncDisposable
    {
        private readonly ServiceProvider provider;
        private readonly string databasePath;

        public DatabaseFixture()
        {
            databasePath = Path.Combine(
                Path.GetTempPath(),
                $"pactracker-tests-{Guid.NewGuid():N}.db");
            var services = new ServiceCollection();
            services.AddInfrastructure(databasePath);

            provider = services.BuildServiceProvider();
            provider.InitializeDatabase();
            Repository = provider.GetRequiredService<IEnergyRepository>();
        }

        public IEnergyRepository Repository { get; }

        public async ValueTask DisposeAsync()
        {
            await provider.DisposeAsync();
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }
}
