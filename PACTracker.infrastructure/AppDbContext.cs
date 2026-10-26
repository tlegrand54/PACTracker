using Microsoft.EntityFrameworkCore;
using PACTracker.Domain.Entities;
using System.Reflection.Emit;

namespace PACTracker.Infrastructure;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Each DbSet maps to a table
    public DbSet<EnergyEntity> Energies => Set<EnergyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}