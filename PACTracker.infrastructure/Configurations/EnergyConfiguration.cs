using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PACTracker.Domain.Entities;

namespace PACTracker.Infrastructure.Configurations;

internal class EnergyConfiguration : IEntityTypeConfiguration<EnergyEntity>
{
    public void Configure(EntityTypeBuilder<EnergyEntity> builder)
    {
        builder.ToTable("Energies");
        builder.HasKey(e => e.Id);

        // One record per day
        builder.HasIndex(e => e.Date).IsUnique();

        // Cop is computed in C#, not stored in the database
        builder.Ignore(e => e.Cop);
    }
}