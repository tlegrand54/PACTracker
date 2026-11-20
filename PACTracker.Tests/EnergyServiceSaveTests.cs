using NSubstitute;
using PACTracker.Core.Energy.Services;
using PACTracker.Core.Exceptions;
using PACTracker.Infrastructure.Repositories;
using Energy = PACTracker.Domain.Entities.EnergyEntity;

namespace PACTracker.Tests;

[TestClass]
public class EnergyServiceSaveTests
{
    private static Energy Reading(DateTime date, double consumption, double production)
        => new Energy{ Date = date, Consumption = consumption, Production = production };

    // Fake repository holding the given readings; returns the ones of the requested month
    private static (EnergyService Service, IEnergyRepository Repository) Create(params Energy[] existing)
    {
        var repository = Substitute.For<IEnergyRepository>();
        repository.GetByMonthAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(call => existing
                .Where(r => r.Date.Year == call.ArgAt<int>(0) && r.Date.Month == call.ArgAt<int>(1))
                .ToList());

        return (new EnergyService(repository), repository);
    }

    [TestMethod]
    public async Task ValueLowerThanEarlierReadingOfSameMonth_IsRejected()
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 10), consumption: 100, production: 300));

        await Assert.ThrowsExactlyAsync<InvalidReadingException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption: 90, production: 320));

        // Nothing must be stored when the reading is rejected
        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task ValueHigherThanLaterReadingOfSameMonth_IsRejected()
    {
        // Late entry: the 15th must not exceed what was already read on the 20th
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 20), consumption: 100, production: 300));

        await Assert.ThrowsExactlyAsync<InvalidReadingException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 15), consumption: 120, production: 280));

        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task EqualValues_AreAccepted()
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 10), consumption: 100, production: 300));

        await service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption: 100, production: 300);

        await repository.Received(1).UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task ZeroValues_AreAccepted()
    {
        var (service, repository) = Create();

        await service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption: 0, production: 0);

        await repository.Received(1).UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task LowerValueInANewMonth_IsAccepted_BecauseTheCounterRestarts()
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 31), consumption: 500, production: 1500));

        await service.SaveRecordAsync(new DateTime(2026, 4, 1), consumption: 5, production: 20);

        await repository.Received(1).UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    [DataRow(double.NaN, 300, "consumption")]
    [DataRow(double.PositiveInfinity, 300, "consumption")]
    [DataRow(double.NegativeInfinity, 300, "consumption")]
    [DataRow(100, double.NaN, "production")]
    [DataRow(100, double.PositiveInfinity, "production")]
    [DataRow(100, double.NegativeInfinity, "production")]
    public async Task NonFiniteValue_IsRejected(
        double consumption,
        double production,
        string parameterName)
    {
        var (service, repository) = Create();

        var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption, production));

        Assert.AreEqual(parameterName, exception.ParamName);
        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    [DataRow(-1, 300, "consumption")]
    [DataRow(100, -1, "production")]
    public async Task NegativeValue_IsRejected(double consumption, double production, string parameterName)
    {
        var (service, repository) = Create();

        var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption, production));

        Assert.AreEqual(parameterName, exception.ParamName);
        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task FutureDate_IsRejectedBeforeReadingOrSaving()
    {
        var (service, repository) = Create();

        var exception = await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
            service.SaveRecordAsync(DateTime.Today.AddDays(1), 100, 300));

        Assert.AreEqual("date", exception.ParamName);
        await repository.DidNotReceive().GetByMonthAsync(Arg.Any<int>(), Arg.Any<int>());
        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    [DataRow(90, 300)]
    [DataRow(100, 290)]
    public async Task ValueLowerThanPreviousReading_IsRejectedIfEitherMetricDecreases(
        double consumption,
        double production)
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 10), 100, 300));

        await Assert.ThrowsExactlyAsync<InvalidReadingException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption, production));

        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    [DataRow(110, 300)]
    [DataRow(100, 310)]
    public async Task ValueHigherThanNextReading_IsRejectedIfEitherMetricIncreases(
        double consumption,
        double production)
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 30), 100, 300));

        await Assert.ThrowsExactlyAsync<InvalidReadingException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption, production));

        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task ReplacementOfExistingDate_IsComparedWithOtherRecords()
    {
        var (service, repository) = Create(
            Reading(new DateTime(2026, 3, 10), consumption: 100, production: 300),
            Reading(new DateTime(2026, 3, 20), consumption: 150, production: 350),
            Reading(new DateTime(2026, 3, 30), consumption: 200, production: 400));

        await service.SaveRecordAsync(new DateTime(2026, 3, 20), consumption: 175, production: 375);

        await repository.Received(1).UpsertAsync(Arg.Is<Energy>(r =>
            r.Date == new DateTime(2026, 3, 20) &&
            r.Consumption == 175 &&
            r.Production == 375));
    }

    [TestMethod]
    public async Task RepositoryFailure_IsPropagatedAndDoesNotTriggerUpsert()
    {
        var repository = Substitute.For<IEnergyRepository>();
        var expectedException = new IOException("Database is unavailable.");
        repository.GetByMonthAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns<Task<IReadOnlyList<Energy>>>(_ => Task.FromException<IReadOnlyList<Energy>>(expectedException));
        var service = new EnergyService(repository);

        var actualException = await Assert.ThrowsExactlyAsync<IOException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), 100, 300));

        Assert.AreSame(expectedException, actualException);
        await repository.DidNotReceive().UpsertAsync(Arg.Any<Energy>());
    }

    [TestMethod]
    public async Task UpsertFailure_IsPropagated()
    {
        var repository = Substitute.For<IEnergyRepository>();
        var expectedException = new IOException("Database is read-only.");
        repository.UpsertAsync(Arg.Any<Energy>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => Task.FromException(expectedException));
        var service = new EnergyService(repository);

        var actualException = await Assert.ThrowsExactlyAsync<IOException>(() =>
            service.SaveRecordAsync(new DateTime(2026, 3, 20), 100, 300));

        Assert.AreSame(expectedException, actualException);
    }
}