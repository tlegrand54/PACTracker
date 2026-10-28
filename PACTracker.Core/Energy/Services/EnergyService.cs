using PACTracker.Core.Energy.DTOs;
using PACTracker.Core.Exceptions;
using PACTracker.Domain.Entities;
using PACTracker.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

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

        public Task SaveRecordAsync(DateTime date, double consumption, double production, CancellationToken ct = default)
        {
            var record = new EnergyEntity { Date = date, Consumption = consumption, Production = production };
            if (record.Consumption < 0 || record.Production < 0)
            {
                ArgumentOutOfRangeException argumentOutOfRangeException = new(nameof(record), "Values must be positive.");
                throw argumentOutOfRangeException;
            }

            // drop the time part so the unique index on Date works
            record.Date = record.Date.Date;

            var monthRecord = GetAllRecordsByMonthAsync(record.Date.Year, record.Date.Month);

            var laterRecord = monthRecord.Result.OrderByDescending(r => r.Date).FirstOrDefault();

            if (laterRecord != null && (laterRecord.Consumption > record.Consumption || laterRecord.Production > record.Production))
                throw new InvalidReadingException("The reading must be higher than the last one of the same month.");

            return repository.UpsertAsync(record, ct);
        }
    }
}
