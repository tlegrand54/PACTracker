using PACTracker.Core.Energy.DTOs;
using PACTracker.Core.Exceptions;
using PACTracker.Domain.Entities;
using PACTracker.Infrastructure.Repositories;

namespace PACTracker.Core.Energy.Services
{
    public class EnergyService : IEnergyService
    {
        private readonly IEnergyRepository repository;

        public EnergyService(IEnergyRepository energyRepository) => repository = energyRepository;

        public async Task<IReadOnlyList<EnergyEntity>> GetAllRecordsByMonthAsync(int year, int month)
        {
            return (await repository.GetByMonthAsync(year, month)).OrderByDescending(o => o.Date).ToList();
        }

        public async Task<YearSummaryDto> GetAllRecordsByYearAsync(int year)
        {
            var yearRecords = await repository.GetByYearAsync(year);
            var monthSummaries = yearRecords
                .GroupBy(r => r.Date.Month)
                .Select(g => new MonthSummaryDto
                {
                    Month = g.Key,
                    TotalProduction = g.OrderByDescending(l => l.Date).FirstOrDefault()?.Production ?? 0,
                    TotalConsumption = g.OrderByDescending(l => l.Date).FirstOrDefault()?.Consumption ?? 0,
                })
                .OrderBy(m => m.Month)
                .ToList();

            return
                new YearSummaryDto
                {
                    Year = year,
                    MonthSummaries = monthSummaries,
                    YearProduction = monthSummaries.Sum(m => m.TotalProduction),
                    YearConsumption = monthSummaries.Sum(m => m.TotalConsumption)

                };
        }

        public EnergyEntity? GetEnergyRecord(int year, int month, int day, IReadOnlyList<EnergyEntity> energyRecords)
        {
            return energyRecords?.FirstOrDefault(w => w.Date.Year == year && w.Date.Month == month && w.Date.Day == day);
        }

        public async Task SaveRecordAsync(DateTime date, double consumption, double production, CancellationToken ct = default)
        {
            if (date.Date > DateTime.Today)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(date),
                    date,
                    "Readings cannot be recorded for a future date.");
            }

            ValidateReadingValue(nameof(consumption), consumption);
            ValidateReadingValue(nameof(production), production);

            var record = new EnergyEntity
            {
                Date = date.Date,
                Consumption = consumption,
                Production = production
            };

            var monthlyRecords = await repository.GetByMonthAsync(
                record.Date.Year,
                record.Date.Month,
                ct);

            var otherRecords = monthlyRecords
                .Where(r => r.Date.Date != record.Date)
                .OrderBy(r => r.Date)
                .ToList();

            var previous = otherRecords.LastOrDefault(r => r.Date < record.Date);
            var next = otherRecords.FirstOrDefault(r => r.Date > record.Date);

            if (previous != null &&
                (record.Consumption < previous.Consumption ||
                 record.Production < previous.Production))
            {
                throw new InvalidReadingException(
                    "The reading must be higher than or equal to the previous reading.");
            }

            if (next != null &&
                (record.Consumption > next.Consumption ||
                 record.Production > next.Production))
            {
                throw new InvalidReadingException(
                    "The reading must be lower than or equal to the next reading.");
            }

            await repository.UpsertAsync(record, ct);
        }

        private static void ValidateReadingValue(string parameterName, double value)
        {
            if (!double.IsFinite(value) || value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Values must be non-negative finite numbers.");
            }
        }
    }
}
