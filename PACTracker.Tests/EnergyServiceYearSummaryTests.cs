using NSubstitute;
using PACTracker.Core.Energy.Services;
using PACTracker.Infrastructure.Repositories;
using Energy = PACTracker.Domain.Entities.EnergyEntity;

namespace PACTracker.Tests
{
    [TestClass]
    public class EnergyServiceYearSummaryTests
    {
        // Single place that builds readings: adapt this line if Energy is a class with setters
        private static Energy Reading(DateTime date, double consumption, double production)
            => new Energy{ Date = date, Consumption = consumption, Production = production };

        // Fake repository that behaves like the database: returns only the readings of the requested year
        private static EnergyService CreateService(params Energy[] readings)
        {
            var repository = Substitute.For<IEnergyRepository>();
            repository.GetByYearAsync(Arg.Any<int>())
                .Returns(call => readings.Where(r => r.Date.Year == call.Arg<int>()).ToList());

            return new EnergyService(repository);
        }

        [TestMethod]
        public async Task SingleReadingInMonth_IsTheMonthTotal()
        {
            var service = CreateService(Reading(new DateTime(2026, 1, 20), consumption: 100, production: 350));

            var result = await service.GetAllRecordsByYearAsync(2026);

            Assert.AreEqual(1, result.MonthSummaries.Count);
            var january = result.MonthSummaries.Single();
            Assert.AreEqual(1, january.Month);
            Assert.AreEqual(100.0, january.TotalConsumption);
            Assert.AreEqual(350.0, january.TotalProduction);
        }

        [TestMethod]
        public async Task SeveralReadingsInMonth_TheLatestDateWins()
        {
            // Entered out of order on purpose: the latest date must win, not the latest insertion
            var service = CreateService(
                Reading(new DateTime(2026, 2, 28), consumption: 130, production: 450),
                Reading(new DateTime(2026, 2, 10), consumption: 40, production: 140),
                Reading(new DateTime(2026, 2, 20), consumption: 90, production: 310));

            var result = await service.GetAllRecordsByYearAsync(2026);

            Assert.AreEqual(1, result.MonthSummaries.Count);
            var february = result.MonthSummaries.Single();
            Assert.AreEqual(130.0, february.TotalConsumption);
            Assert.AreEqual(450.0, february.TotalProduction);
        }

        [TestMethod]
        public async Task ReadingsOnMonthBoundary_BelongToTheirOwnMonth()
        {
            // The counter restarts on the 1st: Feb 1st must not leak into January
            var service = CreateService(
                Reading(new DateTime(2026, 1, 31), consumption: 100, production: 400),
                Reading(new DateTime(2026, 2, 1), consumption: 5, production: 20));

            var result = await service.GetAllRecordsByYearAsync(2026);

            Assert.AreEqual(2, result.MonthSummaries.Count);
            Assert.AreEqual(100.0, result.MonthSummaries.Single(m => m.Month == 1).TotalConsumption);
            Assert.AreEqual(5.0, result.MonthSummaries.Single(m => m.Month == 2).TotalConsumption);
        }

        [TestMethod]
        public async Task MonthsWithoutReadings_AreAbsent_AndMonthsAreOrdered()
        {
            // March is entered before January on purpose, February has nothing
            var service = CreateService(
                Reading(new DateTime(2026, 3, 15), consumption: 80, production: 300),
                Reading(new DateTime(2026, 1, 15), consumption: 60, production: 220));

            var result = await service.GetAllRecordsByYearAsync(2026);

            CollectionAssert.AreEqual(
                new[] { 1, 3 },
                result.MonthSummaries.Select(m => m.Month).ToArray());
        }

        [TestMethod]
        public async Task YearTotals_AreTheSumOfMonthTotals_NotTheSumOfReadings()
        {
            var service = CreateService(
                Reading(new DateTime(2026, 1, 10), consumption: 50, production: 200),
                Reading(new DateTime(2026, 1, 31), consumption: 100, production: 400),
                Reading(new DateTime(2026, 2, 28), consumption: 80, production: 300));

            var result = await service.GetAllRecordsByYearAsync(2026);

            // January counts once (its last reading, 100), not twice (50 + 100)
            Assert.AreEqual(180.0, result.YearConsumption);
            Assert.AreEqual(700.0, result.YearProduction);
        }

        [TestMethod]
        public async Task YearWithoutReadings_ReturnsEmptySummaryWithZeroTotals()
        {
            var service = CreateService();

            var result = await service.GetAllRecordsByYearAsync(2026);

            Assert.AreEqual(2026, result.Year);
            Assert.AreEqual(0, result.MonthSummaries.Count);
            Assert.AreEqual(0.0, result.YearConsumption);
            Assert.AreEqual(0.0, result.YearProduction);
        }

        [TestMethod]
        public async Task ReadingsOfOtherYears_AreIgnored()
        {
            var service = CreateService(
                Reading(new DateTime(2025, 12, 31), consumption: 999, production: 999),
                Reading(new DateTime(2026, 1, 31), consumption: 100, production: 400));

            var result = await service.GetAllRecordsByYearAsync(2026);

            Assert.AreEqual(1, result.MonthSummaries.Count);
            Assert.AreEqual(100.0, result.MonthSummaries.Single().TotalConsumption);
        }
    }
}
