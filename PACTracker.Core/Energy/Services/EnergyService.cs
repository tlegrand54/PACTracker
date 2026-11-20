using PACTracker.Core.Energy.DTOs;
using PACTracker.Core.Exceptions;
using PACTracker.Domain.Entities;
using PACTracker.Infrastructure.Repositories;
using System.Text.Json;

namespace PACTracker.Core.Energy.Services
{
    public class EnergyService : IEnergyService
    {
        private const int DataFormatVersion = 1;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

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

        public async Task<string> ExportDataAsync(CancellationToken ct = default)
        {
            var records = await repository.GetAllAsync(ct);
            var dataFile = new EnergyDataFileDto
            {
                Version = DataFormatVersion,
                Records = records
                    .OrderBy(record => record.Date)
                    .Select(record => new EnergyDataRecordDto(
                        record.Date.Date,
                        record.Consumption,
                        record.Production))
                    .ToList()
            };

            return JsonSerializer.Serialize(dataFile, JsonOptions);
        }

        public async Task<int> ImportDataAsync(string json, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The selected file is empty.");
            }

            EnergyDataFileDto? dataFile;
            try
            {
                dataFile = JsonSerializer.Deserialize<EnergyDataFileDto>(json, JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The selected file is not a valid PACTracker export.", exception);
            }

            if (dataFile is null || dataFile.Version != DataFormatVersion || dataFile.Records is null)
            {
                throw new InvalidDataException("The selected file has an unsupported or invalid format.");
            }

            var records = new List<EnergyEntity>(dataFile.Records.Count);
            var dates = new HashSet<DateTime>();
            foreach (var importedRecord in dataFile.Records)
            {
                if (importedRecord is null)
                {
                    throw new InvalidDataException("The selected file contains an invalid measurement.");
                }

                var date = importedRecord.Date.Date;
                if (importedRecord.Date == default || date > DateTime.Today)
                {
                    throw new InvalidDataException("The selected file contains an invalid measurement date.");
                }

                if (!dates.Add(date))
                {
                    throw new InvalidDataException("The selected file contains more than one measurement for a date.");
                }

                ValidateImportedValue(nameof(importedRecord.Consumption), importedRecord.Consumption);
                ValidateImportedValue(nameof(importedRecord.Production), importedRecord.Production);

                records.Add(new EnergyEntity
                {
                    Date = date,
                    Consumption = importedRecord.Consumption,
                    Production = importedRecord.Production
                });
            }

            await repository.UpsertManyAsync(records, ct);
            return records.Count;
        }

        private static void ValidateImportedValue(string parameterName, double value)
        {
            if (!double.IsFinite(value) || value < 0)
            {
                throw new InvalidDataException($"The selected file contains an invalid {parameterName} value.");
            }
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
