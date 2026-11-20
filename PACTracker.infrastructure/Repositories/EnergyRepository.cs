using Microsoft.EntityFrameworkCore;
using PACTracker.Domain.Entities;

namespace PACTracker.Infrastructure.Repositories;

internal class EnergyRepository(IDbContextFactory<AppDbContext> factory) : IEnergyRepository
{
    public async Task<EnergyEntity?> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Energies.FirstOrDefaultAsync(e => e.Date == date, ct);
    }

    public async Task<IReadOnlyList<EnergyEntity>> GetByMonthAsync(
        int year, int month, CancellationToken ct = default)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        // Short-lived context: created and disposed for each operation
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Energies
            .AsNoTracking() // read-only query: skip change tracking
            .Where(e => e.Date >= start && e.Date < end)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnergyEntity>> GetByYearAsync(int year, CancellationToken ct = default)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);

        // Short-lived context: created and disposed for each operation
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Energies
            .AsNoTracking() // read-only query: skip change tracking
            .Where(e => e.Date >= start && e.Date < end)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnergyEntity>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Energies
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(EnergyEntity energy, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(energy);
        ValidateReadingValue(nameof(energy.Consumption), energy.Consumption);
        ValidateReadingValue(nameof(energy.Production), energy.Production);
        energy.Date = energy.Date.Date;

        await using var db = await factory.CreateDbContextAsync(ct);

        var existing = await db.Energies.FirstOrDefaultAsync(e => e.Date == energy.Date, ct);

        if (existing is null)
        {
            db.Energies.Add(energy);
        }
        else
        {
            // The context tracks "existing": changing its properties is enough
            existing.Consumption = energy.Consumption;
            existing.Production = energy.Production;
        }

        // Generates and runs the INSERT or UPDATE
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertManyAsync(
        IReadOnlyCollection<EnergyEntity> energies,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(energies);

        var records = energies.Select(energy =>
        {
            ArgumentNullException.ThrowIfNull(energy);
            ValidateReadingValue(nameof(energy.Consumption), energy.Consumption);
            ValidateReadingValue(nameof(energy.Production), energy.Production);

            return new EnergyEntity
            {
                Date = energy.Date.Date,
                Consumption = energy.Consumption,
                Production = energy.Production
            };
        }).ToArray();

        await using var db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        foreach (var record in records)
        {
            var existing = await db.Energies.FirstOrDefaultAsync(e => e.Date == record.Date, ct);
            if (existing is null)
            {
                db.Energies.Add(record);
            }
            else
            {
                existing.Consumption = record.Consumption;
                existing.Production = record.Production;
            }
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
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
