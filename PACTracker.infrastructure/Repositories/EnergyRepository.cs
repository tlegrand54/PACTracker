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

    public async Task UpsertAsync(EnergyEntity energy, CancellationToken ct = default)
    {
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
}