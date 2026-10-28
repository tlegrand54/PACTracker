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
    public async Task LowerValueInANewMonth_IsAccepted_BecauseTheCounterRestarts()
    {
        var (service, repository) = Create(Reading(new DateTime(2026, 3, 31), consumption: 500, production: 1500));

        await service.SaveRecordAsync(new DateTime(2026, 4, 1), consumption: 5, production: 20);

        await repository.Received(1).UpsertAsync(Arg.Any<Energy>());
    }
}